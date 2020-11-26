using System;

namespace BatteryMax
{
    public static class Extensions
    {
        public static int ToInt(this double dble)
        {
            return Convert.ToInt32(dble);
        }
    }
}
