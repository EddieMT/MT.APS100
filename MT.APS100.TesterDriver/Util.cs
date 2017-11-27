using System;
using System.Diagnostics;

namespace MT.APS100.TesterDriver
{
    public static class Util
    {
        public static void WaitTime(double settlingTime)
        {
            Stopwatch timer = new Stopwatch();
            long neededTicks = (long)Math.Floor(settlingTime * Stopwatch.Frequency);

            timer.Restart();

            do
            {
            } while (timer.ElapsedTicks < neededTicks);

            timer.Stop();
        }

        public const string USERCAL_DIR = @"C:\MerlinTest\UserCal\";

        public const int DEFAULT_TEST_RESULT = -999999999;
    }
}
