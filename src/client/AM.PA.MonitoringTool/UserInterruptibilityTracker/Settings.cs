using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterruptibilityTracker
{
    public class Settings
    {
        public static int PopupIntervalMin = 10;
        public static int PopupIntervalMax = 40;
        public const string DbTableInterruptibilityPopup = "user_interruptibility_survey";
        public static TimeSpan IntervalPostpone1h = TimeSpan.FromHours(1);
        public static TimeSpan IntervalPostpone2h = TimeSpan.FromHours(2);
        public static TimeSpan TimerCheckInterval = TimeSpan.FromMinutes(1);
    }
}
