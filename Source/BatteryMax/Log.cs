using System;
using System.IO;
using System.Reflection;

namespace BatteryMax
{
    public static class Log
    {
        private static readonly string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "batterymax.log");

        public static void Write(string str)
        {
            File.AppendAllText(path, $"{str}{Environment.NewLine}");
        }
    }
}
