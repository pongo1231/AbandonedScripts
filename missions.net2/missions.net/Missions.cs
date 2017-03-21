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
using static CitizenFX.Core.UI.Screen;

namespace missions.net
{
    public class Missions : BaseScript
    {
        public Missions()
        {
            startMission();

            Tick += new Func<Task>(async delegate
            {
                await Task.FromResult(0);
                Vector3 pos = Game.PlayerPed.Position;
                UIResText posText = new UIResText($"{pos.X} {pos.Y} {pos.Z}", new PointF(1280, 3), 0.5f);
                posText.Draw();
            });
        }

        private void startMission()
        {
            CarMissions carMissions = new CarMissions();
            carMissions.startMission1();
        }
    }
}
