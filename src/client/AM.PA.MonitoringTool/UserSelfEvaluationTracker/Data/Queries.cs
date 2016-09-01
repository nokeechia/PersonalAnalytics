using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserSelfEvaluationTracker.Models;
using Shared;
using Shared.Data;
using System.Globalization;
using System.Data;


namespace UserSelfEvaluationTracker.Data
{
    class Queries
    {
        /// <summary>
        /// Create Table if not exists
        /// </summary>
        internal static void CreateSelfEvaluationTables()
        {
            Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableIntervalPopup + " (id INTEGER PRIMARY KEY, time TEXT, surveyNotifyTime TEXT, surveyStartTime TEXT, surveyEndTime TEXT, userEngagement NUMBER, userAttention NUMBER)");
        }

        /// <summary>
        /// Saves the survey entry to the database
        /// </summary>
        /// <param name="entry"></param>
        internal static void SaveIntervalEntry(SelfEvaluationSurveyEntry entry)
        {
            if (entry == null) return;

            try
            {
                var query = "INSERT INTO " + Settings.DbTableIntervalPopup + " (time, surveyNotifyTime, surveyStartTime, surveyEndTime, userEngagement, userAttention) VALUES " +
                            "(strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                            Database.GetInstance().QTime(entry.TimeStampNotification) + ", " +
                            Database.GetInstance().QTime(entry.TimeStampStarted) + ", " +
                            Database.GetInstance().QTime(entry.TimeStampFinished) + ", " +
                            Database.GetInstance().Q(entry.Engagement.ToString(CultureInfo.InvariantCulture)) + ", " +
                            Database.GetInstance().Q(entry.Attention.ToString(CultureInfo.InvariantCulture)) + ");";

                Database.GetInstance().ExecuteDefaultQuery(query);
            }
            catch { }
        }

        /// <summary>
        /// returns the previous survey entry if available
        /// (only get interval survey responses)
        /// </summary>
        /// <returns>previous survey entry or null, if there isn't any</returns>
        internal static SelfEvaluationSurveyEntry GetPreviousIntervalSurveyEntry()
        {
            var query = "SELECT surveyNotifyTime, surveyStartTime, surveyEndTime, userEngagement, userAttention FROM '" + Settings.DbTableIntervalPopup + "' ORDER BY time DESC;";
            var res = Database.GetInstance().ExecuteReadQuery(query);
            if (res == null || res.Rows.Count == 0) return null;

            var entry = new SelfEvaluationSurveyEntry();

            if (res.Rows[0]["surveyNotifyTime"] != null)
            {
                try
                {
                    var val = DateTime.Parse((string)res.Rows[0]["surveyNotifyTime"], CultureInfo.InvariantCulture);
                    entry.TimeStampNotification = val;
                }
                catch { } // necessary, if we run it after the DB initialization, there is no value
            }
            if (res.Rows[0]["surveyStartTime"] != null)
            {
                try
                {
                    var val = DateTime.Parse((string)res.Rows[0]["surveyStartTime"], CultureInfo.InvariantCulture);
                    entry.TimeStampStarted = val;
                }
                catch { } // necessary, if we run it after the DB initialization, there is no value
            }
            if (res.Rows[0]["surveyEndTime"] != null)
            {
                try
                {
                    var val = DateTime.Parse((string)res.Rows[0]["surveyEndTime"], CultureInfo.InvariantCulture);
                    entry.TimeStampFinished = val;
                }
                catch { } // necessary, if we run it after the DB initialization, there is no value
            }
            if (res.Rows[0]["userEngagement"] != null)
            {
                try
                {
                    var val = Convert.ToInt32(res.Rows[0]["userEngagement"], CultureInfo.InvariantCulture);
                    entry.Engagement = val;
                }
                catch { } // necessary, if we run it after the DB initialization, there is no value
            }
            if (res.Rows[0]["userAttention"] != null)
            {
                try
                {
                    var val = Convert.ToInt32(res.Rows[0]["userAttention"], CultureInfo.InvariantCulture);
                    entry.Attention = val;
                }
                catch { } // necessary, if we run it after the DB initialization, there is no value
            }
            return entry;
        }

        /// <summary>
        /// Returns a dictionary with the attention and engagement on a timeline
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static List<Tuple<DateTime, int, int>> GetSelfEvaluationTimelineData(DateTimeOffset date, VisType type, bool withNonWork = false)
        {
            var evalList = new List<Tuple<DateTime, int, int>>();

            try
            {
                var filterNonWork = (withNonWork) ? "" : " AND userEngagement <> -1 AND userAttention <> -1";

                var query = "SELECT userEngagement, userAttention, surveyEndTime FROM " + Settings.DbTableIntervalPopup + " " + // end time is the time the participant answered
                                      "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(type, date, "surveyNotifyTime") + " " + // only show perceived productivity values for the day
                                      filterNonWork +
                                      " ORDER BY surveyEndTime;";
                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var time = DateTime.Parse((string)row["surveyEndTime"], CultureInfo.InvariantCulture);
                    var engagement = Convert.ToInt32(row["userEngagement"], CultureInfo.InvariantCulture);
                    var attention = Convert.ToInt32(row["userAttention"], CultureInfo.InvariantCulture);

                    // first element
                    if (evalList.Count == 0)
                    {
                        var workDayStartTime = Database.GetInstance().GetUserWorkStart(date);
                        evalList.Add(new Tuple<DateTime, int, int>(workDayStartTime, engagement, attention));
                    }

                    // only show if it's from today
                    if (time.Date == date.Date)
                    {
                        evalList.Add(new Tuple<DateTime, int, int>(time, engagement, attention));
                    }
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return evalList;
        }
    }


}
