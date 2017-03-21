using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace missions.net
{
    public class BackupHandler : BaseScript
    {
        RelationshipGroup bodyguardGroup;

        public BackupHandler()
        {
            bodyguardGroup = World.AddRelationshipGroup("bodyguards");
            bodyguardGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Companion, true);

            EventHandlers["bc:buzzardbackup"] += new Action(SpawnBuzzardBackup);
            EventHandlers["bc:technicalbackup"] += new Action(SpawnTechnicalBackup);
        }

        private async void SpawnBuzzardBackup()
        {
            Vector3 spawnPos = World.GetSafeCoordForPed(Game.PlayerPed.Position);
            spawnPos.Y += 200;
            await Delay(10000);

            Vehicle heli = await World.CreateVehicle(VehicleHash.Buzzard2, spawnPos);
            heli.IsEngineRunning = true;
            heli.IsCollisionEnabled = false; // so the heli doesn't crash into stuff
            Blip heliBlip = heli.AttachBlip();
            heliBlip.Sprite = BlipSprite.HelicopterAnimated;
            heliBlip.Color = BlipColor.Blue;

            Ped pilot = await createPed(PedHash.Blackops01SMY, new Vector3());
            pilot.RelationshipGroup = bodyguardGroup;
            pilot.Weapons.Give(WeaponHash.CombatMG, int.MaxValue, true, true);
            pilot.ShootRate = int.MaxValue;
            pilot.Accuracy = 95;
            pilot.Armor = 200;
            pilot.CanRagdoll = false;
            pilot.CanWrithe = false;
            pilot.SetIntoVehicle(heli, VehicleSeat.Driver);
            pilot.Task.ChaseWithHelicopter(Game.PlayerPed, new Vector3(0, 0, 20));

            Ped bodyguard1 = await createPed(PedHash.Blackops01SMY, new Vector3());
            bodyguard1.RelationshipGroup = bodyguardGroup;
            bodyguard1.Weapons.Give(WeaponHash.CombatMG, int.MaxValue, true, true);
            bodyguard1.ShootRate = int.MaxValue;
            bodyguard1.Accuracy = 95;
            bodyguard1.Armor = 200;
            bodyguard1.CanRagdoll = false;
            bodyguard1.CanWrithe = false;
            bodyguard1.SetIntoVehicle(heli, VehicleSeat.LeftRear);

            Ped bodyguard2 = await createPed(PedHash.Blackops01SMY, new Vector3());
            bodyguard2.RelationshipGroup = bodyguardGroup;
            bodyguard2.Weapons.Give(WeaponHash.CombatMG, int.MaxValue, true, true);
            bodyguard2.ShootRate = int.MaxValue;
            bodyguard2.Accuracy = 95;
            bodyguard2.Armor = 200;
            bodyguard2.CanRagdoll = false;
            bodyguard2.CanWrithe = false;
            bodyguard2.SetIntoVehicle(heli, VehicleSeat.RightRear);

            // Wait until heli is nearby
            while (Vector3.Distance(heli.Position, Game.PlayerPed.Position) > 50)
            {
                await Delay(1);
            }

            heli.IsCollisionEnabled = true; // Now he can gladly crash into shit

            // 5 minute countdown
            for (int i = 300; i > 0; i--)
            {
                if (heli.Health < 1
                 && pilot.IsDead
                 && bodyguard1.IsDead
                 && bodyguard2.IsDead)
                {
                    break;
                }

                await Delay(1000);
            }

            heli.AttachedBlip.Alpha = 0;
            pilot.Task.FleeFrom(Game.PlayerPed);

            heli.MarkAsNoLongerNeeded();
            pilot.MarkAsNoLongerNeeded();
            bodyguard1.MarkAsNoLongerNeeded();
            bodyguard2.MarkAsNoLongerNeeded();
        }

        private async void SpawnTechnicalBackup()
        {
            Vector3 spawnPos = Game.PlayerPed.GetOffsetPosition(World.GetNextPositionOnStreet(Game.PlayerPed.Position));
            await Delay(10000);

            Vehicle technical = await World.CreateVehicle(VehicleHash.Technical, spawnPos);
            technical.IsEngineRunning = true;
            technical.CanTiresBurst = false;
            technical.IsBulletProof = true;
            technical.LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
            Blip technicalBlip = technical.AttachBlip();
            technicalBlip.Sprite = BlipSprite.GunCar;
            technicalBlip.Color = BlipColor.Blue;

            Ped driver = await createPed(PedHash.Blackops01SMY, new Vector3());
            driver.RelationshipGroup = bodyguardGroup;
            driver.Weapons.Give(WeaponHash.CarbineRifle, int.MaxValue, true, true);
            driver.ShootRate = int.MaxValue;
            driver.Accuracy = 95;
            driver.Armor = 200;
            driver.CanRagdoll = false;
            driver.CanWrithe = false;
            driver.DrivingStyle = DrivingStyle.AvoidTraffic;
            driver.Task.FollowToOffsetFromEntity(Game.PlayerPed, new Vector3(), 200, int.MaxValue, 10, true);
            driver.SetIntoVehicle(technical, VehicleSeat.Driver);

            Ped gunman = await createPed(PedHash.Blackops01SMY, new Vector3());
            gunman.RelationshipGroup = bodyguardGroup;
            gunman.Weapons.Give(WeaponHash.CarbineRifle, int.MaxValue, true, true);
            gunman.ShootRate = int.MaxValue;
            gunman.Accuracy = 95;
            gunman.Armor = 200;
            gunman.CanRagdoll = false;
            gunman.CanWrithe = false;
            gunman.SetIntoVehicle(technical, VehicleSeat.LeftRear);

            for (int i = 300000; i > 0; i--)
            {
                if (gunman.IsDead)
                {
                    break;
                }

                // So they can't exit
                if (driver.CurrentVehicle == null
                 || gunman.CurrentVehicle == null)
                {
                    driver.SetIntoVehicle(technical, VehicleSeat.Driver);
                    gunman.SetIntoVehicle(technical, VehicleSeat.LeftRear);
                }

                technicalBlip.Rotation = (int) Math.Ceiling(technical.Heading);

                await Delay(1);
            }

            technical.AttachedBlip.Alpha = 0;
            driver.Task.FleeFrom(Game.PlayerPed);

            technical.MarkAsNoLongerNeeded();
            driver.MarkAsNoLongerNeeded();
            gunman.MarkAsNoLongerNeeded();
        }

        private async Task<Ped> createPed(Model model, Vector3 pos, float heading = 0)
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
    }
}
