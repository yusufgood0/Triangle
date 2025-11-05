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
        DateTime startTime;
        int callCount;
        public int FPS = 0;
        public void callTimer()
        {
            callCount++;
        }
        public bool checkForReset()
        {
            if (RunTime > 1)
            {
                FPS = callCount;
                startTime = DateTime.Now;
                callCount = 0;
                return true;
            }
            return false;
        }
        public bool update()
        {
            callTimer();
            return checkForReset();
        }

        double RunTime { get => (DateTime.Now - startTime).TotalSeconds; }
    }
}
