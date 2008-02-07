using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TypingManager
{
    public class QueryTime
    {
        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceCounter(ref long x);
        
        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceFrequency(ref long x);
    
        public static double Now {
            get {
                long cnt = 0;
                long frq = 0;
                QueryPerformanceCounter(ref cnt);
                QueryPerformanceFrequency(ref frq);
                double c = (double)cnt / (double)frq;
                return c;
            }
        }

        public static int NowMiliSec
        {
            get
            {
                return (int)(QueryTime.Now * 1000);
            }
        }
    }
}
