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
        public void CallTimer()
        {
            callCount++;
        }
        public bool CheckForReset()
        {
            if (RunTime >= 1)
            {
                FPS = callCount;
                startTime = DateTime.Now;
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

        double RunTime { get => (DateTime.Now - startTime).TotalSeconds; }
    }
}
