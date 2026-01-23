using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeGame
{
    internal class FramesPerSecondTimer
    {
        DateTime startTime = Game1.PlayingGameTime;
        int callCount;
        public int FPS = 0;
        public void CallTimer()
        {
            callCount++;
        }
        public bool CheckForReset()
        {
            if (startTime <= Game1.PlayingGameTime)
            {
                FPS = callCount;
                startTime = startTime.AddSeconds(1);
                callCount = 0;
                return true;
            }
            return false;
        }
        public bool Update()
        {
            CallTimer();
            return CheckForReset();
        }

        //double RunTime { get => (Game1.PlayingGameTime - startTime).TotalSeconds; }
    }
}
