using CitizenFX.Core;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace missions.net
{
    public class CarMissions : BaseScript
    {
        private Mission mission;

        public CarMissions()
        {
            mission = new Mission();
        }

        public async void startMission1()
        {
            Player player = Game.Player;
            Ped playerPed = Game.PlayerPed;
            MissionMusic music = new MissionMusic();

            music.playCalmMusic();

            Vehicle vehicle = await World.CreateVehicle(VehicleHash.Comet2, MissionPosition.CarHouse);

            Blip blip = mission.attachBlip(vehicle);
            blip.Color = BlipColor.Blue;

            mission.showMessage("Get the ~b~Comet~w~.");

            while (playerPed.CurrentVehicle != vehicle)
            {
                await Delay(1);
                if (vehicle.EngineHealth < 1 || !vehicle.Exists())
                {
                    mission.stopMission(music, "~r~The vehicle's engine is broken.", vehicle);
                    return;
                }
            }

            blip.Alpha = 0;

            Vector3 pos = MissionPosition.Terminal;

            mission.drawMarker(pos, Color.FromArgb(180, 0, 0, 255));

            Blip markerBlip = mission.showBlip(pos);
            markerBlip.Color = BlipColor.Blue;

            mission.showMessage("Bring the ~b~Comet~w~ to the ~b~Terminal~w~.");

            Vector3 playerPos = playerPed.Position;
            while (Math.Abs(Vector3.Distance(playerPos, pos)) > 5)
            {
                await Delay(1);
                if (vehicle.EngineHealth < 1 || !vehicle.Exists())
                {
                    mission.stopMission(music, "~r~The vehicle's engine is broken.", vehicle);
                    return;
                }
                playerPos = playerPed.Position;
            }
            markerBlip.Alpha = 0;

            Ped enemy1 = await mission.createEnemyPed(PedHash.Robber01SMY, new Vector3(1050, -3285, 5), WeaponHash.AssaultRifle);
            Ped enemy2 = await mission.createEnemyPed(PedHash.Robber01SMY, new Vector3(1060, -3295, 5), WeaponHash.AssaultRifle);
            Ped enemy3 = await mission.createEnemyPed(PedHash.Robber01SMY, new Vector3(1060, -3292, 5), WeaponHash.AssaultRifle);
            music.playActionMusic();
            mission.showMessage("Watch out for the ~r~enemies~w~.");

            while (enemy1.Health > 0 || enemy2.Health > 0 || enemy3.Health > 0)
            {
                await Delay(1);
            }

            mission.stopMission(music, "You won!", vehicle, enemy1, enemy2, enemy3);
        }
    }
}
