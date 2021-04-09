using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace BatteryMax
{
    public static class Log
    {
        private static readonly string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "batterymax.log");

        public static void Write(string template, params object[] values)
        {
#if DEBUG
            var now = DateTime.Now;
            var sb = new StringBuilder();

            sb.Append(now.ToShortDateString()).Append(' ');
            sb.Append(now.ToShortTimeString()).Append(" - ");
            sb.AppendFormat(template, values);
            sb.AppendLine();

            File.AppendAllText(path, sb.ToString());
#endif
        }
    }
}
