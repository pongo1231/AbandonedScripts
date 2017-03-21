using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace missions.net
{
    public class Mission : BaseScript
    {
        RelationshipGroup relationship;

        public Mission()
        {
            relationship = new RelationshipGroup("MISSION_ENEMY".GetHashCode());
            relationship.SetRelationshipBetweenGroups(relationship, Relationship.Respect);

            Ped playerPed = Game.PlayerPed;
            relationship.SetRelationshipBetweenGroups(playerPed.RelationshipGroup, Relationship.Hate);
            playerPed.RelationshipGroup.SetRelationshipBetweenGroups(relationship, Relationship.Hate);
        }

        public void showMessage(string message)
        {
            Screen.ShowSubtitle(message, 5000);
        }

        public void drawMarker(Vector3 pos, Color color)
        {
            World.DrawMarker(MarkerType.VerticalCylinder, pos, new Vector3(), new Vector3(), new Vector3(15f), color);
        }

        public Blip showBlip(Vector3 pos)
        {
            return World.CreateBlip(pos);
        }

        public Blip attachBlip(Entity entity)
        {
            return entity.AttachBlip();
        }

        public async Task<Ped> createPed(Model model, Vector3 pos, float heading = 0)
        {
            int hash = model.Hash;
            Function.Call(Hash.REQUEST_MODEL, hash);
            while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash))
            {
                await Delay(1);
            }

            int pedId = Function.Call<int>(Hash.CREATE_PED, 26, hash, pos.X, pos.Y, pos.Z, heading, true, true);
            return new Ped(pedId);
        }

        public async Task<Ped> createEnemyPed(Model model, Vector3 pos, WeaponHash weaponHash = WeaponHash.Unarmed, float heading = 0)
        {
            int hash = model.Hash;
            Function.Call(Hash.REQUEST_MODEL, hash);
            while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash))
            {
                await Delay(1);
            }

            int pedId = Function.Call<int>(Hash.CREATE_PED, 26, hash, pos.X, pos.Y, pos.Z, heading, true, true);
            Ped ped = new Ped(pedId);
            ped.Weapons.Give(weaponHash, 99999, true, true);

            ped.RelationshipGroup = relationship;
            ped.Task.FightAgainst(Game.PlayerPed);

            return ped;
        }

        public void stopMission(MissionMusic music, string reason, params Entity[] entities)
        {
            music.stopMusic();
            foreach (Entity entity in entities)
            {
                Blip blip = entity.AttachedBlip;
                if (blip != null)
                {
                    blip.Alpha = 0;
                }
                entity.Delete();
            }
            showMessage(reason);
        }
    }
}
