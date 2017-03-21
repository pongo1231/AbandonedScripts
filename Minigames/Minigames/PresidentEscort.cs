using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minigames
{
    class PresidentEscort : BaseScript
    {
        private bool gameRunning = false;
        private string group;

        private RelationshipGroup presidentGroup;
        private RelationshipGroup terroristGroup;
        private RelationshipGroup bodyguardGroup;

        public PresidentEscort()
        {
            presidentGroup = World.AddRelationshipGroup("president");
            terroristGroup = World.AddRelationshipGroup("terrorist");
            bodyguardGroup = World.AddRelationshipGroup("bodyguard");

            presidentGroup.SetRelationshipBetweenGroups(terroristGroup, Relationship.Hate);
            presidentGroup.SetRelationshipBetweenGroups(bodyguardGroup, Relationship.Companion);
            terroristGroup.SetRelationshipBetweenGroups(presidentGroup, Relationship.Hate);
            terroristGroup.SetRelationshipBetweenGroups(bodyguardGroup, Relationship.Hate);
            bodyguardGroup.SetRelationshipBetweenGroups(presidentGroup, Relationship.Companion);
            bodyguardGroup.SetRelationshipBetweenGroups(terroristGroup, Relationship.Hate);

            EventHandlers["playerSpawned"] += new Action<dynamic>(respawn);
            EventHandlers["pe:host"] += new Action(startHost);
            EventHandlers["pe:start"] += new Action<string>(startPlayer);
            EventHandlers["pe:stopGame"] += new Action(cleanupGame);
        }

        private void startHost()
        {
            Screen.ShowNotification("Host");
        }

        private void startPlayer(string group)
        {
            this.group = group;

            if (group == "president")
            {
                Screen.ShowSubtitle("You are the ~y~President~w~. Survive until the time runs up and watch out for ~r~terrorists~w~.", 10000);
                spawnPresident();
            }
            else if (group == "terrorist")
            {
                Screen.ShowSubtitle("You are a ~r~Terrorist~w~. Kill the ~y~President~w~ before the time runs up.", 10000);
                spawnTerrorist();
            }
            else if (group == "bodyguard")
            {
                Screen.ShowSubtitle("You are a ~b~Bodyguard~w~. Protect the ~y~President~w~ from ~r~Terrorists~w~ until the time runs up.", 10000);
                spawnBodyguard();
            }

            gameRunning = true;

            checkPresidentDeath();
            // Check Blips
            for (int i = 0; i < 64; i++)
            {
                if (!Function.Call<bool>(Hash.NETWORK_IS_PLAYER_ACTIVE, i) || i == Function.Call<int>(Hash.PLAYER_ID))
                {
                    continue;
                }

                int pedId = Function.Call<int>(Hash.GET_PLAYER_PED, i);
                Ped ped = new Ped(pedId);

                if (group == "president")
                {
                    if (ped.RelationshipGroup == bodyguardGroup)
                    {
                        Blip blip = ped.AttachBlip();
                        blip.Sprite = BlipSprite.Standard;
                        blip.Color = BlipColor.Blue;
                    }
                }
                else if (group == "terrorist")
                {
                    if (ped.RelationshipGroup == presidentGroup)
                    {
                        Blip blip = ped.AttachBlip();
                        blip.Sprite = BlipSprite.Standard;
                        blip.Color = BlipColor.Yellow;
                    }
                    else if (ped.RelationshipGroup == terroristGroup)
                    {
                        Blip blip = ped.AttachBlip();
                        blip.Sprite = BlipSprite.Standard;
                        blip.Color = BlipColor.Red;
                    }
                }
                else if (group == "bodyguard")
                {
                    if (ped.RelationshipGroup == presidentGroup)
                    {
                        Blip blip = ped.AttachBlip();
                        blip.Sprite = BlipSprite.Standard;
                        blip.Color = BlipColor.Yellow;
                    }
                    else if (ped.RelationshipGroup == bodyguardGroup)
                    {
                        Blip blip = ped.AttachBlip();
                        blip.Sprite = BlipSprite.Standard;
                        blip.Color = BlipColor.Blue;
                    }
                }
            }
        }

        private void spawnPresident()
        {
            Game.PlayerPed.RelationshipGroup = presidentGroup;

            Game.PlayerPed.Position = new Vector3(-1167, -1749, 3);
            //Game.Player.ChangeModel(PedHash.Business01AMY);
            Game.PlayerPed.Weapons.Give(WeaponHash.Pistol, 300, true, true);
            Game.PlayerPed.Armor = 200;

            Function.Call(Hash.PREPARE_MUSIC_EVENT, "OJDA5_START");
            Function.Call(Hash.TRIGGER_MUSIC_EVENT, "OJDA5_START");
        }

        private void spawnTerrorist()
        {
            Game.PlayerPed.RelationshipGroup = terroristGroup;

            Game.PlayerPed.Position = new Vector3(890, -1872, 30);
            //Game.Player.ChangeModel(new Model(PedHash.Robber01SMY));
            Game.PlayerPed.Weapons.Give(WeaponHash.Pistol, 600, false, true);
            Game.PlayerPed.Weapons.Give(WeaponHash.AssaultRifle, 1500, true, true);

            Function.Call(Hash.PREPARE_MUSIC_EVENT, "OJDA5_START");
            Function.Call(Hash.TRIGGER_MUSIC_EVENT, "OJDA5_START");
        }

        private void spawnBodyguard()
        {
            Game.PlayerPed.RelationshipGroup = bodyguardGroup;

            Game.PlayerPed.Position = new Vector3(-1167, -1749, 3);
            //Game.Player.ChangeModel(PedHash.Business01AMY);
            Game.PlayerPed.Weapons.Give(WeaponHash.Pistol, 600, true, true);
            Game.PlayerPed.Weapons.Give(WeaponHash.CarbineRifle, 1500, true, true);
            Game.PlayerPed.Armor = 200;

            Function.Call(Hash.PREPARE_MUSIC_EVENT, "OJDA5_START");
            Function.Call(Hash.TRIGGER_MUSIC_EVENT, "OJDA5_START");
        }

        private void respawn(dynamic spawn)
        {
            if (!gameRunning)
            {
                return;
            }

            if (group == "president")
            {
                spawnPresident();
            }
            else if (group == "terrorist")
            {
                spawnTerrorist();
            }
            else if (group == "bodyguard")
            {
                spawnBodyguard();
            }
        }

        private async void checkPresidentDeath()
        {
            while (gameRunning)
            {
                await Delay(1);

                if (Game.PlayerPed.Health < 1)
                {
                    gameRunning = false;
                    TriggerServerEvent("pe:stopGame", "pDead");
                }
            }
        }

        private void cleanupGame()
        {
            gameRunning = false;
            group = null;
            Function.Call(Hash.CANCEL_MUSIC_EVENT, "OJDA5_START");
        }
    }
}
