using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomodoro
{
    public class Pomodoro
    {
        public Pomodoro()
        {
            PausedResumed = new List<Tuple<string, DateTime>>();
            Task = string.Empty;
        }

        public int ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public List<Tuple<string, DateTime>> PausedResumed { get; set; }
        public string Task { get; set; }
    }
}
