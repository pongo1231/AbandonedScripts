using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace missions.net
{
    public class MissionMusic : BaseScript
    {
        private string[] calmMusic = new string[]
        {
            "APT_YA_DEFENDING"
        };

        private string[] actionMusic = new string[]
        {
            "APT_YA_ACTION"
        };

        private int choosedMusic;

        public MissionMusic()
        {
            Random random = new Random();
            choosedMusic = random.Next(calmMusic.Length - 1);
        }

        public void playCalmMusic()
        {
            Function.Call(Hash.TRIGGER_MUSIC_EVENT, calmMusic[choosedMusic]);
        }

        public void playActionMusic()
        {
            Function.Call(Hash.TRIGGER_MUSIC_EVENT, actionMusic[choosedMusic]);
        }

        public void stopMusic()
        {
            Function.Call(Hash.CANCEL_MUSIC_EVENT, calmMusic[choosedMusic]);
            Function.Call(Hash.CANCEL_MUSIC_EVENT, actionMusic[choosedMusic]);
        }
    }
}
