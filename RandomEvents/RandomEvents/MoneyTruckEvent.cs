using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Threading.Tasks;

namespace RandomEvents {
    class MoneyTruckEvent : BaseScript {
        private Func<Task> onTickTask;
        private RelationshipGroup moneyTruckerGroup;
        private bool alreadySpawned = false;

        private bool playerCapturedTruck = false;

        private TimerBarPool timerBarPool;

        private Blip destBlip = null;
        private Player prevTruckOwner = null;

        private Vehicle truck;
        private Blip truckBlip;
        private Ped driver;
        private Ped guard;

        public MoneyTruckEvent() {
            EventHandlers["playerSpawned"] += new Action<dynamic>(spawn => {
                if (!alreadySpawned) {
                    onTickTask = new Func<Task>(OnTick);

                    moneyTruckerGroup = World.AddRelationshipGroup("moneyTrucker");
                    moneyTruckerGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Dislike, true);
                    moneyTruckerGroup.SetRelationshipBetweenGroups(new RelationshipGroup("COP".GetHashCode()), Relationship.Companion, true);

                    alreadySpawned = true;
                }
            });

            EventHandlers["re:preparemoneytruckevent"] += new Action(PrepareMoneyTruckEvent);

            EventHandlers["re:startmoneytruckevent"] += new Action<dynamic, dynamic, dynamic>(StartMoneyTruckEvent);
        }

        private async void PrepareMoneyTruckEvent() {
            playerCapturedTruck = false;
            destBlip = null;
            prevTruckOwner = null;

            truck = await World.CreateVehicle(VehicleHash.Stockade, new Vector3(-1683, 505, 127), 110);
            truck.IsEngineRunning = true;
            truck.CanTiresBurst = false;
            truck.EngineHealth = 10000;

            driver = await SpawnPed(PedHash.Armoured01, new Vector3());
            driver.Armor = 100;
            driver.Task.DriveTo(truck, new Vector3(-45, -763, 32), 5, 200);
            driver.AlwaysKeepTask = true;
            driver.RelationshipGroup = moneyTruckerGroup;
            driver.SetIntoVehicle(truck, VehicleSeat.Driver);

            guard = await SpawnPed(PedHash.Armoured01, new Vector3());
            guard.Armor = 100;
            guard.Weapons.Give(WeaponHash.Pistol, int.MaxValue, true, true);
            guard.RelationshipGroup = moneyTruckerGroup;
            guard.SetIntoVehicle(truck, VehicleSeat.RightFront);

            TriggerServerEvent("re:startmoneytruckevent", truck.NativeValue, driver.NativeValue, guard.NativeValue);
        }

        private async void StartMoneyTruckEvent(dynamic truckNative, dynamic driverNative, dynamic guardNative) {
            RandomEvents.SetGameRunning(true);

            timerBarPool = new TimerBarPool();
            TextTimerBar timer = new TextTimerBar("Time Left", "7:00");
            timerBarPool.Add(timer);

            truck = new Vehicle(truckNative);
            driver = new Ped(driverNative);
            guard = new Ped(guardNative);

            truckBlip = truck.AttachBlip();
            truckBlip.Sprite = (BlipSprite)67;
            truckBlip.Color = BlipColor.Blue;

            Function.Call(Hash.FLASH_MINIMAP_DISPLAY);

            Tick += onTickTask;

            for (int i = 0; i < 1000; i++) {
                await Delay(1);
                Screen.DisplayHelpTextThisFrame("A money truck has been exposed to the Minimap System. "
                    + "Capture it for money.");
            }
        }

        public async Task OnTick() {
            if (!playerCapturedTruck) {
                foreach (Player player in Players) {
                    if (player.Character.CurrentVehicle == truck) {
                        playerCapturedTruck = true;
                        player.WantedLevel = 4;
                        break;
                    }
                }
            } else if (playerCapturedTruck) {
                bool playerIsDriver = truck.GetPedOnSeat(VehicleSeat.Driver) == LocalPlayer.Character;

                if (playerIsDriver && LocalPlayer != prevTruckOwner) {
                    destBlip = World.CreateBlip(new Vector3(1174, 2643, 37));
                    World.WaypointPosition = destBlip.Position;
                    prevTruckOwner = LocalPlayer;

                    Screen.ShowSubtitle("Bring the ~b~money truck~w~ to the safehouse before the authories can detect it.", 5000);
                } else if (playerIsDriver
                    && destBlip != null) {
                    destBlip.Alpha = 0;
                }

                if (World.GetDistance(truck.Position, new Vector3(1174, 2643, 37)) < 2) {
                    LocalPlayer.WantedLevel = 0;
                    destBlip.Alpha = 0;

                    StopEvent("~b~Money truck captured!~w~");
                    return;
                }
            }

            timerBarPool.Draw();

            bool playerTruckClose = World.GetDistance(Game.PlayerPed.Position, truck.Position) < 450;
            bool musicPlaying = Function.Call<bool>(Hash.AUDIO_IS_SCRIPTED_MUSIC_PLAYING);

            if (playerTruckClose && !musicPlaying) {
                //Function.Call(Hash.TRIGGER_MUSIC_EVENT, "FH2B_MISSION_START");
                //Function.Call(Hash.TRIGGER_MUSIC_EVENT, "FH2B_NOOSE_FIGHT");
                Function.Call(Hash.TRIGGER_MUSIC_EVENT, "AH3A_START");
                Function.Call(Hash.TRIGGER_MUSIC_EVENT, "AH3A_START_ESCAPE");
            } else if (!playerTruckClose) {
                //Function.Call(Hash.TRIGGER_MUSIC_EVENT, "FH2B_MISSION_END");
                Function.Call(Hash.TRIGGER_MUSIC_EVENT, "AH3A_STOP");
            }

            if (truck.EngineHealth == 0) {
                StopEvent("~r~Failed: Money truck has been destroyed.");
                return;
            }

            await Task.FromResult(0);
        }

        private void StopEvent(string reason) {
            truckBlip.Alpha = 0;
            truck.MarkAsNoLongerNeeded();
            driver.MarkAsNoLongerNeeded();
            guard.MarkAsNoLongerNeeded();

            Function.Call(Hash.TRIGGER_MUSIC_EVENT, "FH2B_MISSION_END");

            TriggerServerEvent("re:stopmoneytruckevent");
            RandomEvents.SetGameRunning(false);

            playerCapturedTruck = false;
            Tick -= onTickTask;
            Screen.ShowSubtitle(reason, 5000);
        }

        private async Task<Ped> SpawnPed(Model model, Vector3 pos, float heading = 0) {
            int hash = model.Hash;
            Function.Call(Hash.REQUEST_MODEL, hash);
            while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash)) {
                await Delay(1);
            }

            int pedId = Function.Call<int>(Hash.CREATE_PED, 26, hash, pos.X, pos.Y, pos.Z, heading, true, true);
            return new Ped(pedId);
        }
    }
}