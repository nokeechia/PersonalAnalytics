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
                "userInterruptibility NUMBER, " +
                "postponed TEXT)"
            );
        }

        internal static void SaveInterruptibilityEntry(InterruptibilitySurveyEntry entry)
        {
            if (entry == null) return;

            try
            {
                var query = "INSERT INTO " + Settings.DbTableInterruptibilityPopup +
                    " (time, surveyNotifyTime, surveyStartTime, surveyEndTime, userInterruptibility, postponed) VALUES " +
                            "(strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                            Database.GetInstance().QTime(entry.TimeStampNotification) + ", " +
                            Database.GetInstance().QTime(entry.TimeStampStarted) + ", " +
                            Database.GetInstance().QTime(entry.TimeStampFinished) + ", " +
                            Database.GetInstance().Q(entry.Interruptibility.ToString(CultureInfo.InvariantCulture)) + ", " +
                            Database.GetInstance().Q(entry.Postponed.ToString()) + ");";

                Database.GetInstance().ExecuteDefaultQuery(query);
            }
            catch { }
        }
    }
}
