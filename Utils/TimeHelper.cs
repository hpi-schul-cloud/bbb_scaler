using System.Globalization;
using System;

namespace HPI.BBB.Autoscaler.Utils
{
    public class TimeHelper
    {
        private static readonly string TIMEZONE = ConfigReader.GetValue("TIMEZONE", "DEFAULT", "TIMEZONE");
        private static readonly int START_H = int.Parse(ConfigReader.GetValue("START_H", "DEFAULT", "START_H"), CultureInfo.InvariantCulture);
        private static readonly int START_MIN = int.Parse(ConfigReader.GetValue("START_MIN", "DEFAULT", "START_MIN"), CultureInfo.InvariantCulture);
        private static readonly int END_H = int.Parse(ConfigReader.GetValue("END_H", "DEFAULT", "END_H"), CultureInfo.InvariantCulture);
        private static readonly int END_MIN = int.Parse(ConfigReader.GetValue("END_MIN", "DEFAULT", "END_MIN"), CultureInfo.InvariantCulture);
        private static readonly int MINIMUM_ACTIVE_MACHINES_ON = int.Parse(ConfigReader.GetValue("MINIMUM_ACTIVE_MACHINES_ON", "DEFAULT", "MINIMUM_ACTIVE_MACHINES_ON"), CultureInfo.InvariantCulture);
        private static readonly int MINIMUM_ACTIVE_MACHINES_OFF = int.Parse(ConfigReader.GetValue("MINIMUM_ACTIVE_MACHINES_OFF", "DEFAULT", "MINIMUM_ACTIVE_MACHINES_OFF"), CultureInfo.InvariantCulture);
        

        internal static bool IsOn()
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE);
            TimeSpan start = new TimeSpan(START_H, START_MIN, 0);
            TimeSpan end = new TimeSpan(END_H, END_MIN, 0);
            TimeSpan now = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).TimeOfDay;

            return ((now > start) && (now < end));
        }

        internal static int GetMinimumActiveMachines()
        {
            if (TimeHelper.IsOn())
            {
                return MINIMUM_ACTIVE_MACHINES_ON;
            }
            else
            {
                return MINIMUM_ACTIVE_MACHINES_OFF;
            }
        }

    }
}
