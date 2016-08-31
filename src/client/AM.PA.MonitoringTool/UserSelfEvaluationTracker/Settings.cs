using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserSelfEvaluationTracker
{
    public static class Settings
    {
        public static bool DefaultPopUpIsEnabled = true;
        public const int DefaultPopUpInterval = 60; // in minutes

        public const string DbTableIntervalPopup = "user_self_evaluation_survey";

        private const double IntervalPostponeShortInMinutes = 5.0; // every 5mins
        private const double SurveyCheckerMinutes = 1.0; // every minute
        private const double IntervalCloseIfNotAnsweredAfterHours = 2.0; // close survey if not answered after 2 hours

        public static TimeSpan IntervalPostponeShortInterval = TimeSpan.FromMinutes(IntervalPostponeShortInMinutes);
        public static TimeSpan SurveyCheckerInterval = TimeSpan.FromMinutes(SurveyCheckerMinutes);
        public static TimeSpan IntervalCloseIfNotAnsweredInterval = TimeSpan.FromHours(IntervalCloseIfNotAnsweredAfterHours);
        public static TimeSpan DailyPopUpEarliestMoment = new TimeSpan(4, 0, 0); // 4 am in the morning

    }
}
