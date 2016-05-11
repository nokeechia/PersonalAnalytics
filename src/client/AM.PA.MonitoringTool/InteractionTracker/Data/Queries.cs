// Created by André Meyer at UZH and Paige Rodeghero
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Data;

namespace InteractionTracker.Data
{
    public class Queries
    {
        /// <summary>
        /// Returns the focused time for the last hour
        /// (which is 60min - time spent in Outlook, Skype, Lync)
        /// </summary>
        /// <returns></returns>
        internal static int GetFocusTimeInLastHour()
        {
            try
            {
                var today = DateTime.Now;

                var query = "SELECT (3600 - SUM(difference)) as 'difference'"
                          + "FROM ( "
                          + "SELECT (strftime('%s', t2.time) - strftime('%s', t1.time)) as 'difference' " 
                          + "FROM " + Settings.WindowsActivityTable + " t1 LEFT JOIN " + Settings.WindowsActivityTable + " t2 on t1.id + 1 = t2.id "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t1.time") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t2.time") + " "
                          + "AND TIME(t2.time) between TIME('" + today.AddHours(-1).ToString("HH:mm") + "') and TIME('" + today.ToString("HH:mm") +"') "
                          + "AND ( LOWER(t1.process) == 'outlook' or LOWER(t1.process) == 'skype' or LOWER(t1.process) == 'lync')"
                          + "GROUP BY t1.id, t1.time "
                          + "ORDER BY difference DESC)";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var difference = Convert.ToInt32(row["difference"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return difference;
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// Gets the number of switches to Outlook, Skype, Skype for business
        /// for the last hour (times 2, because there is the switch back to work)
        /// within the last hour
        /// </summary>
        /// <returns></returns>
        internal static int GetNumInteractionSwitches()
        {
            try
            {
                var today = DateTime.Now;

                var query = "SELECT (2 * COUNT(*)) as 'totalSwitches'"
                              + "FROM " + Settings.WindowsActivityTable + " t1 LEFT JOIN " + Settings.WindowsActivityTable + " t2 on t1.id + 1 = t2.id "
                              + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t1.time") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, today.Date, "t2.time") + " "
                              + "AND TIME(t2.time) between TIME('" + today.AddHours(-1).ToString("HH:mm") + "') and TIME('" + today.ToString("HH:mm") + "') "
                              + "AND ( LOWER(t1.process) == 'outlook' or LOWER(t1.process) == 'skype' or LOWER(t1.process) == 'lync')"
                              + "AND (strftime('%s', t2.time) - strftime('%s', t1.time)) > 3;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var difference = Convert.ToInt32(row["totalSwitches"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return difference;
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// Counts the number of meetings for today
        /// </summary>
        /// <returns>returns the number of meetings for today</returns>
        internal static int GetMeetingsForLastHour()
        {
            // get the meetings
            var meetings = new List<Tuple<DateTime, DateTime>>();
            try
            {
                var query = "SELECT time, durationInMins FROM " + Shared.Settings.MeetingsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, DateTime.Now.Date) + ";";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timeStart = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);
                    var duration = Convert.ToInt32(row["durationInMins"], CultureInfo.InvariantCulture);
                    var timeEnd = timeStart.AddMinutes(duration);

                    var t = new Tuple<DateTime, DateTime>(timeStart, timeEnd);
                    meetings.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            // count up the meetings in the last hour
            var count = 0;
            var startTs = DateTime.Now.AddHours(-1);
            var endTs = DateTime.Now;
            foreach (var meeting in meetings)
            {
                if (meeting.Item1 > startTs && meeting.Item2 < endTs) count += 2; // meeting occurs only during the hour
                else if (meeting.Item1 < startTs && meeting.Item2 < endTs) count++; // meeting starts before the hour and ends during the hour
                else if (meeting.Item1 > startTs && meeting.Item2 > endTs) count++; // meeting starts during the hour and ends after the hour
            }

            return count;
        }

        /// <summary>
        /// Counts the number of meetings for today
        /// </summary>
        /// <returns>returns the number of meetings for today</returns>
        internal static int GetMeetingsForDate(DateTimeOffset date)
        {
            try
            {
                var answer = "SELECT COUNT(*) FROM " + Settings.MeetingsTable + " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date.Date) + ";";
                var count = Database.GetInstance().ExecuteScalar(answer);
                return count;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return 0;
            }
        }

        /// <summary>
        /// The emails sent or received
        /// </summary>
        /// <returns>Tuple item1: sent, item2: received</returns>
        /// <summary>
        /// The emails sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <param name="sentOrReceived"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        public static int GetSentOrReceivedEmails(DateTimeOffset date, string sentOrReceived)
        {
            try
            {
                var query = "SELECT time, " + sentOrReceived + " FROM " + Settings.EmailsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                            + "ORDER BY time DESC "
                            + "LIMIT 1;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var answer = Convert.ToInt32(row[sentOrReceived], CultureInfo.InvariantCulture);
                   
                    table.Dispose();
                    return answer;
                }
                else
                {
                    table.Dispose();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return -1;
            }
        }

        /// <summary>
        /// The calls sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        internal static int GetCallsOrChats(DateTimeOffset date, string tableName)
        {
            try
            {
                var answer = "SELECT COUNT(*) FROM " + tableName + " WHERE " +
                             Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, DateTime.Now.Date) + " AND (sent == 1 or received == 1);";
                var count = Database.GetInstance().ExecuteScalar(answer);
                return count;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return 0;
            }
        }
    }
}
