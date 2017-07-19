using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomodoro
{
    public class Settings
    {
        internal static readonly string PomodoroTable = "pomodoros";
        internal static readonly string DateFormat = "yyyy-MM-dd HH:mm:ss";
        internal static readonly string DateDayFormat = "yyyy-MM-dd";
        internal static readonly int DefaultPomodoroDuration = 1; //for testing, later put to 25
        internal static readonly int DefaultShortBreakDuration = 5;
        internal static readonly int DefaultLongBreakDuration = 15;
    }
}
