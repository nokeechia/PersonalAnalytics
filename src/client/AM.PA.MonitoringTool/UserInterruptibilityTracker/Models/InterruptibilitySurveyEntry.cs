using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterruptibilityTracker.Models
{
    public class InterruptibilitySurveyEntry
    {
        public DateTime TimeStampNotification { get; set; }
        public DateTime TimeStampStarted { get; set; }
        public DateTime TimeStampFinished { get; set; }
        public int Interruptibility { get; set; } // 1-5

        public InterruptibilitySurveyEntry()
        {
            TimeStampStarted = DateTime.Now;
        }
    }
}
