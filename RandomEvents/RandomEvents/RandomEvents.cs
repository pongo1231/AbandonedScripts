using CitizenFX.Core;
using System;
using System.Threading.Tasks;

namespace RandomEvents {
    public class RandomEvents : BaseScript {
        private bool alreadySpawned = false;
        private static bool gameRunning = false;

        public RandomEvents() {
            EventHandlers["playerSpawned"] += new Action<dynamic>(spawn => {
                if (!alreadySpawned) {
                    Tick += new Func<Task>(async delegate {
                        if (!gameRunning) {
                            Random rand = new Random();
                            int chance = rand.Next(1);
                            if (chance == 0) {
                                TriggerServerEvent("re:preparemoneytruckevent");
                            }

                            await Task.FromResult(0);
                        }
                    });

                    alreadySpawned = true;
                }
            });
        }

        public static void SetGameRunning(bool state) {
            gameRunning = state;
        }

        public static bool IsGameRunning() {
            return gameRunning;
        }
    }
}
