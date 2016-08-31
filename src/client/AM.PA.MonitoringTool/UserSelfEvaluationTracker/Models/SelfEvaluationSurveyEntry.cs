using System;

namespace UserSelfEvaluationTracker.Models
{
    public class SelfEvaluationSurveyEntry
    {
        public DateTime PreviousWorkDay { get; set; }
        public DateTime TimeStampNotification { get; set; }
        public DateTime TimeStampStarted { get; set; }
        public DateTime TimeStampFinished { get; set; }

        public int Engagement { get; set; } // 1-7
        public int Attention { get; set; } // 1-7


        public SelfEvaluationSurveyEntry()
        {
            TimeStampStarted = DateTime.Now;
        }
    }
}
