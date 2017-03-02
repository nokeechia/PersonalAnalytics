using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Data;
using UserInterruptibilityTracker.Models;
using System.Globalization;

namespace UserInterruptibilityTracker.Data
{
    public class Queries
    {
        internal static void CreateUserInterruptibilityTables()
        {
            Database.GetInstance().ExecuteDefaultQuery(
                "CREATE TABLE IF NOT EXISTS " + Settings.DbTableInterruptibilityPopup + " (" +
                "id INTEGER PRIMARY KEY, " +
                "time TEXT, " +
                "surveyNotifyTime TEXT, " +
                "surveyStartTime TEXT, " +
                "surveyEndTime TEXT, " +
                "userInterruptibility NUMBER )"
            );
        }

        /// <summary>
        /// returns the previous survey entry if available
        /// (only get interval survey responses)
        /// </summary>
        /// <returns>previous survey entry or null, if there isn't any</returns>
        internal static InterruptibilitySurveyEntry GetPreviousInterruptibilitySurveyEntry()
        {
            var query = "SELECT surveyNotifyTime, surveyStartTime, surveyEndTime, userInterruptibility FROM '" + Settings.DbTableInterruptibilityPopup + "' ORDER BY time DESC;";
            var res = Database.GetInstance().ExecuteReadQuery(query);
            if (res == null || res.Rows.Count == 0) return null;

            var entry = new InterruptibilitySurveyEntry();

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
            if (res.Rows[0]["userInterruptibility"] != null)
            {
                try
                {
                    var val = Convert.ToInt32(res.Rows[0]["userInterruptibility"], CultureInfo.InvariantCulture);
                    entry.Interruptibility = val;
                }
                catch { } // necessary, if we run it after the DB initialization, there is no value
            }
            return entry;
        }

        internal static void SaveInterruptibilityEntry(InterruptibilitySurveyEntry entry)
        {
            if (entry == null) return;

            try
            {
                var query = "INSERT INTO " + Settings.DbTableInterruptibilityPopup +
                    " (time, surveyNotifyTime, surveyStartTime, surveyEndTime, userInterruptibility) VALUES " +
                            "(strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                            Database.GetInstance().QTime(entry.TimeStampNotification) + ", " +
                            Database.GetInstance().QTime(entry.TimeStampStarted) + ", " +
                            Database.GetInstance().QTime(entry.TimeStampFinished) + ", " +
                            Database.GetInstance().Q(entry.Interruptibility.ToString(CultureInfo.InvariantCulture)) + ");";

                Database.GetInstance().ExecuteDefaultQuery(query);
            }
            catch { }
        }
    }
}
