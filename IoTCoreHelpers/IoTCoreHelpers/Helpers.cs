using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCoreHelpers
{
    class Helpers
    {

        public class Waiting
        {
            private Stopwatch stopwatch = Stopwatch.StartNew();
            //A synchronous wait is used to avoid yielding the thread 
            //This method calculates the number of CPU ticks will elapse in the specified time and spins
            //in a loop until that threshold is hit. This allows for very precise timing.
            public void Wait(double milliseconds)
            {
                long initialTick = stopwatch.ElapsedTicks;
                long initialElapsed = stopwatch.ElapsedMilliseconds;
                double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
                double finalTick = initialTick + desiredTicks;
                while (stopwatch.ElapsedTicks < finalTick)
                {
                    //nothing than waiting
                }
            }

            public long MillisecCounter()
            {
                return stopwatch.ElapsedMilliseconds;
            }
        }
        public static byte[] UshortToByte(ushort[] inbuff)
        {
            byte[] outbuff = new byte[inbuff.Length * 2];
            for (int i = 0; i < inbuff.Length; i++)
            {
                outbuff[i * 2 + 1] = (byte)(inbuff[i] & (ushort)0xFF);
                outbuff[i * 2] = (byte)((inbuff[i] & (ushort)0xFF00) >> 8);
            }
            return outbuff;
        }


    }
}
