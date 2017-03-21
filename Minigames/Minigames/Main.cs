using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minigames
{
    public class Main : BaseScript
    {
        private bool alreadySpawned = false;
        private static bool showJoinCam = true;

        public Main()
        {
            EventHandlers["playerSpawned"] += new Action<dynamic>(handleSpawn);
            EventHandlers["attachWaitCam"] += new Action<string>(attachWaitCam);
            EventHandlers["deattachWaitCam"] += new Action(deattachWaitCam);

            Tick += new Func<Task>(async delegate
            {
                if (!showJoinCam)
                {
                    drawPosText();
                }

                if (Game.IsControlJustReleased(1, Control.InteractionMenu))
                {
                    deattachWaitCam();
                }
            });
        }

        private async void handleSpawn(dynamic spawn)
        {
            if (!alreadySpawned)
            {
                await Delay(500);

                TriggerServerEvent("newPlayer", Function.Call<int>(Hash.PLAYER_ID));
                alreadySpawned = true;
            }
        }

        private void attachWaitCam(string loadingText)
        {
            Screen.LoadingPrompt.Show(loadingText);

            Game.PlayerPed.Position = new Vector3(-75, -820, 400);
            Game.PlayerPed.IsPositionFrozen = true;
            Game.PlayerPed.IsCollisionEnabled = false;
            Game.PlayerPed.CanRagdoll = false;
            Game.PlayerPed.IsInvincible = true;
            Game.PlayerPed.IsVisible = false;

            Game.PlayerPed.Health = 200;
            Game.PlayerPed.Armor = 0;
            Game.PlayerPed.Weapons.RemoveAll();

            Screen.Hud.IsRadarVisible = false;

            Function.Call(Hash.SET_MOBILE_RADIO_ENABLED_DURING_GAMEPLAY, true);
            Game.RadioStation = RadioStation.SoulwaxFM;
        }

        private async void deattachWaitCam()
        {
            if (showJoinCam)
            {
                showJoinCam = false;

                Screen.LoadingPrompt.Hide();

                Game.PlayerPed.IsCollisionEnabled = true;
                Game.PlayerPed.CanRagdoll = true;
                Game.PlayerPed.IsInvincible = false;
                Game.PlayerPed.IsVisible = true;
                Game.PlayerPed.IsPositionFrozen = false;

                Screen.Hud.IsRadarVisible = true;

                Function.Call(Hash.SET_MOBILE_RADIO_ENABLED_DURING_GAMEPLAY, false);
            }
        }

        private void drawPosText()
        {
            Vector3 pos = Game.PlayerPed.Position;
            UIResText posText = new UIResText($"{pos.X} {pos.Y} {pos.Z}", new PointF(1280, 3), 0.5f);
            posText.Draw();
        }
    }
}
