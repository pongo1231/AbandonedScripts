using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruckerJob
{
    public class TruckerJob : BaseScript
    {
        private bool isTrucker = false;

        private dynamic[] spawns =
        {
            new Vector3(-1751, 1998, 116)
        };
        private dynamic[] goals =
        {
            new Vector3(1729, 3317, 40)
        };

        private void AddNonTruckerMenu(UIMenu menu)
        {
            menu.Clear();

            UIMenuItem startItem = new UIMenuItem("Start Job", "Begin your journey.");
            menu.AddItem(startItem);

            UIMenuItem aboutItem = new UIMenuItem("Version: 1.0", "Made by Mr. Scammer");
            menu.AddItem(aboutItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == startItem)
                {
                    StartJob(menu);
                }
            };
        }

        private async void StartJob(UIMenu menu)
        {
            AddTruckerMenu(menu);
            showJobNotification("~b~Deliver the items.", 5000);
            isTrucker = true;

            dynamic[] newStuff = await NewPickup(); 
            Pickup pickup = newStuff[0];
            Blip blip = newStuff[1];

            Tick += new Func<Task>(async delegate
            {
                await Task.FromResult(0);
                if (!isTrucker)
                {
                    return;
                }

                if (!pickup.Exists())
                {
                    showJobNotification("The item has been lost. A new delivery has been tasked.", 5000);
                    blip.Alpha = 0;
                    blip.Delete();

                    newStuff = await NewPickup();
                    pickup = newStuff[0];
                    blip = newStuff[1];
                }
                else if (pickup.IsCollected)
                {
                    //showJobNotification("Collected.", 5000);
                }
            });
        }

        private async Task<dynamic[]> NewPickup()
        {
            Random random = new Random();
            Vector3 spawnPos = spawns[random.Next(spawns.Length - 1)];
            Pickup pickup = await World.CreatePickup(PickupType.CustomScript, spawnPos, new Model("prop_money_bag_01"), 1);
            Blip blip = World.CreateBlip(spawnPos);
            blip.Sprite = BlipSprite.GTAOMission;
            blip.ShowRoute = true;

            return new dynamic[]
            {
                pickup,
                blip
            };
        }

        private void AddTruckerMenu(UIMenu menu)
        {
            menu.Clear();

            UIMenuItem startItem = new UIMenuItem("Stop Job", "Stop your journey for now.");
            menu.AddItem(startItem);

            UIMenuItem aboutItem = new UIMenuItem("Version: 1.0", "Made by Mr. Scammer");
            menu.AddItem(aboutItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == startItem)
                {
                    // Stop Job
                    AddNonTruckerMenu(menu);
                }
            };
        }

        public TruckerJob()
        {
            MenuPool menuPool = new MenuPool();

            UIMenu menu = new UIMenu("Carrier Menu", "");
            menuPool.Add(menu);

            AddNonTruckerMenu(menu);

            menuPool.RefreshIndex();

            Tick += new Func<Task>(async delegate
            {
                // Debug shit
                //Vector3 pos = LocalPlayer.Character.Position;
                //Screen.DisplayHelpTextThisFrame($"X:{pos.X} Y:{pos.Y} Z:{pos.Z}");

                // Menu stuff
                menuPool.ProcessMenus();
                if (Game.IsControlJustReleased(1, Control.InteractionMenu))
                {
                    menu.Visible = !menu.Visible;
                }

                await Task.FromResult(0);
            });
        }

        private async void showJobNotification(string text, int milli)
        {
            int i = milli;
            Tick += new Func<Task>(async delegate
            {
                await Task.FromResult(0);
                i--;

                Screen.DisplayHelpTextThisFrame(text);

                if (i < 1)
                {
                    return;
                }
            });
        }
    }
}
