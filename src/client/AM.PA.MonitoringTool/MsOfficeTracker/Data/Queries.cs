// Created by André Meyer at MSR
// Created: 2015-11-12
// 
// Licensed under the MIT License.
//
// Modified by Paige Rodeghero (paige.rodeghero@us.abb.com) from ABB USCRC

using MsOfficeTracker.Helpers;
using Shared;
using Shared.Data;
using System;
using System.Globalization;
using System.Data.SqlClient;

namespace MsOfficeTracker.Data
{
    public class Queries
    {
        internal static void CreateMsTrackerTables()
        {
            try
            {
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.EmailsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, inbox INTEGER, sent INTEGER, received INTEGER, isFromTimer INTEGER)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.MeetingsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, subject TEXT, durationInMins INTEGER)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.ChatsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, service TEXT, sent INTEGER, received INTEGER, isFromTimer INTEGER)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.CallsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, service TEXT, sent INTEGER, received INTEGER, isFromTimer INTEGER)");
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        /// <summary>
        /// Returns the total time a user spent in Outlook
        /// </summary>
        /// <param name="date"></param>
        /// <returns>time in minutes</returns>
        internal static double TimeSpentInOutlook(DateTimeOffset date)
        {
            try
            {
                var query = "SELECT SUM(difference) as difference "
                            + "FROM ("
                            + "SELECT (strftime('%s', t2.time) - strftime('%s', t1.time)) as 'difference' " //t1.id, t1.time as 'from', t2.time as 'to'
                            + "FROM " + Shared.Settings.WindowsActivityTable + " t1 LEFT JOIN " + Shared.Settings.WindowsActivityTable + " t2 on t1.id + 1 = t2.id "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "t1.time") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "t2.time") + " "
                            + "AND lower(t1.process) = 'outlook' "
                            + "GROUP BY t1.id, t1.time "
                            + "ORDER BY difference DESC "
                            + ");";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    try
                    {
                        var row = table.Rows[0];
                        var difference = Convert.ToInt32(row["difference"], CultureInfo.InvariantCulture);
                        return difference / 60.0;
                    }
                    catch // (InvalidCastException e)
                    {
                        // don't do anything
                        return -1;
                    }
                    finally
                    {
                        table.Dispose();
                    }
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
    /// Class calls the API to determine the inbox size, number of emails sent and received.
    /// (inbox size: is only collected for date = DateTime.Now)
    /// </summary>
    /// <param name="date"></param>
    /// <param name="isFromTimer"></param>
    internal static Tuple<int, int> CreateEmailsSnapshot(DateTime date, bool isFromTimer)
        {
            try
            {
                // get inbox size (can only be done today)
                var inboxSize = -1;
                if (date.Date == DateTime.Now.Date)
                {
                    date = DateTime.Now; // add time to save full time stamp
                    var inboxSizeResponse = Office365Api.GetInstance().GetNumberOfEmailsInInbox();
                    inboxSizeResponse.Wait();
                    inboxSize = (int)inboxSizeResponse.Result;
                }

                // get emails sent count
                var emailsSentResult = Office365Api.GetInstance().GetNumberOfEmailsSent(date.Date);
                emailsSentResult.Wait();
                var emailsSent = emailsSentResult.Result;

                // get emails received count
                var emailsReceivedResult = Office365Api.GetInstance().GetNumberOfEmailsReceived(date.Date);
                emailsReceivedResult.Wait();
                var emailsReceived = emailsReceivedResult.Result;

                // save into the database
                SaveEmailsSnapshot(date, inboxSize, emailsSent, emailsReceived, isFromTimer);

                // return for immediate use
                return new Tuple<int, int>(emailsSent, emailsReceived);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return new Tuple<int, int>(-1, -1);
            }
        }

       
        /// <summary>
        /// Saves the timestamp, inbox size, sent items count, received items count into the database
        /// </summary>
        /// <param name="window"></param>
        /// <param name="process"></param>
        internal static void SaveEmailsSnapshot(DateTime date, long inbox, int sent, int received, bool isFromTimer)
        {
            Database.GetInstance().ExecuteDefaultQuery("INSERT INTO " + Settings.EmailsTable + " (timestamp, time, inbox, sent, received, isFromTimer) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                Database.GetInstance().QTime(date) + ", " + Database.GetInstance().Q(inbox) + ", " + Database.GetInstance().Q(sent) + ", " + Database.GetInstance().Q(received) + ", " + Database.GetInstance().Q(isFromTimer) + ");");
        }

        /// <summary>
        /// Saves the timestamp, subject and duration in minutes to the database
        /// only if the meeting is not yet stored for a given day
        /// 
        /// Hint: The meeting subject can be obfuscated by setting the property RecordMeetingTitles to false
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="durationInMins"></param>
        internal static void SaveMeetingsSnapshot(DateTime date, string subject, int durationInMins)
        {
            if (Shared.Settings.AnonymizeSensitiveData)
            {
                subject = Dict.Anonymized;  // obfuscate window title
            }

            var query = "INSERT INTO " + Settings.MeetingsTable + " (timestamp, time, subject, durationInMins) "
                        + "SELECT strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " + Database.GetInstance().QTime(date) + ", " + Database.GetInstance().Q(subject) + ", " + Database.GetInstance().Q(durationInMins) + " "
                        + "WHERE NOT EXISTS ("
                            + "SELECT 1 FROM " + Settings.MeetingsTable + " WHERE time = " + Database.GetInstance().QTime(date) + " AND subject = " + Database.GetInstance().Q(subject) + " AND durationInMins = " + Database.GetInstance().Q(durationInMins)
                        + ");";

            Database.GetInstance().ExecuteDefaultQuery(query);
        }

        /// <summary>
        /// Check if there is already meetings stored for the date
        /// </summary>
        /// <returns>true if yes, false otherwise</returns>
        internal static bool HasMeetingEntriesForDate(DateTime date)
        {
            try
            {
                var query = "SELECT EXISTS( "
                          + "SELECT 1 FROM " + Settings.MeetingsTable + " "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + "); ";

                var count = Database.GetInstance().ExecuteScalar(query);
                return (count != 0);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return false;
            }
        }

        /// <summary>
        /// Counts the number of meetings for today
        /// </summary>
        /// <returns>returns the number of meetings for today</returns>
        internal static int GetMeetingsForDate(DateTimeOffset date)
        {
            try
            {
                var answer = "SELECT COUNT(*) FROM " + Settings.MeetingsTable + " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + ";";
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
        /// Removes all stored meetings for date so they can be updated
        /// (necessary in case someone deletes a meeting)
        /// </summary>
        /// <param name="date"></param>
        internal static void RemoveMeetingsForDate(DateTimeOffset date)
        {
            var query = "DELETE FROM " + Settings.MeetingsTable + " WHERE time = " + Database.GetInstance().QTime(date.Date) + ";";
            Database.GetInstance().ExecuteDefaultQuery(query);
        }

        /// <summary>
        /// Check if there is already emails (with isFromTimer = 0) stored for the date
        /// </summary>
        /// <returns>true if yes, false otherwise</returns>
        internal static bool HasEmailsEntriesForDate(DateTimeOffset date, bool isFromTimer)
        {
            try
            {
                var query = "SELECT EXISTS( "
                          + "SELECT 1 FROM " + Settings.EmailsTable + " "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                          + "AND isFromTimer = " + Database.GetInstance().Q(isFromTimer) + ");";

                var count = Database.GetInstance().ExecuteScalar(query);
                if (count == 0) return false;
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return false;
            }
        }
       
        /// <summary>
        /// The emails sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <param name="sentOrReceived"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        internal static Tuple<DateTime, int> GetSentOrReceivedEmails(DateTimeOffset date, string sentOrReceived)
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
                    var timestamp = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return new Tuple<DateTime, int>(timestamp, answer);
                }
                else
                {
                    table.Dispose();
                    return new Tuple<DateTime, int>(DateTime.MinValue, -1);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return new Tuple<DateTime, int>(DateTime.MinValue, -1);
            }
        }

        /// <summary>
        /// Class calls the API to determine the number of calls sent and received.
        /// (inbox size: is only collected for date = DateTime.Now)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="isFromTimer"></param>
        internal static Tuple<int, int, int, int> CreateChatsAndCallsSnapshot(DateTime date, bool isFromTimer)
        {
            try
            {
                // get chats and calls sent and received count
                var numbersResult = Office365Api.GetInstance().GetNumberOfChatsAndCallsSentOrReceived(date.Date);
                numbersResult.Wait();
                var numbers = numbersResult.Result;

                var chatsSent = numbers.Item1;
                var chatsReceived = numbers.Item2;
                var callsSent = numbers.Item3;
                var callsReceived = numbers.Item4;

                // save chats into database
                SaveChatsSnapshot(date, chatsSent, chatsReceived, isFromTimer);

                // save calls into the database
                SaveCallsSnapshot(date, callsSent, callsReceived, isFromTimer);

                // return for immediate use
                return new Tuple<int, int, int, int>(chatsSent, chatsReceived, callsSent, callsReceived);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return new Tuple<int, int, int, int>(-1, -1, -1, -1);
            }
        }

        /// <summary>
        /// Saves the timestamp, inbox size, sent items count, received items count into the database
        /// </summary>
        /// <param name="window"></param>
        /// <param name="process"></param>
        internal static void SaveChatsSnapshot(DateTime date, int sent, int received, bool isFromTimer)
        {
            Database.GetInstance().ExecuteDefaultQuery("INSERT INTO " + Settings.ChatsTable + " (timestamp, time, service, sent, received, isFromTimer) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                Database.GetInstance().QTime(date) + ", " + "'skype'" + ", " + Database.GetInstance().Q(sent) + ", " + Database.GetInstance().Q(received) + ", " + Database.GetInstance().Q(isFromTimer) + ");");
        }

        /// <summary>
        /// Check if there is already chats (with isFromTimer = 0) stored for the date
        /// </summary>
        /// <returns>true if yes, false otherwise</returns>
        internal static bool HasChatsEntriesForDate(DateTimeOffset date, bool isFromTimer)
        {
            try
            {
                var query = "SELECT EXISTS( "
                          + "SELECT 1 FROM " + Settings.ChatsTable + " "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                          + "AND isFromTimer = " + Database.GetInstance().Q(isFromTimer) + ");";

                var count = Database.GetInstance().ExecuteScalar(query);
                if (count == 0) return false;
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return false;
            }
        }

        /// <summary>
        /// The chats sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <param name="sentOrReceived"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        internal static Tuple<DateTime, int> GetSentOrReceivedChats(DateTimeOffset date, string sentOrReceived)
        {
            try
            {
                var query = "SELECT time, " + sentOrReceived + " FROM " + Settings.ChatsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                            + "ORDER BY time DESC "
                            + "LIMIT 1;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var answer = Convert.ToInt32(row[sentOrReceived], CultureInfo.InvariantCulture);
                    var timestamp = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return new Tuple<DateTime, int>(timestamp, answer);
                }
                else
                {
                    table.Dispose();
                    return new Tuple<DateTime, int>(DateTime.MinValue, -1);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return new Tuple<DateTime, int>(DateTime.MinValue, -1);
            }
        }

        /// <summary>
        /// Saves the timestamp, service, sent items count, received items count into the database
        /// </summary>
        /// <param name="date"></param>
        /// <param name="sent"></param>
        /// <param name="received"></param>
        internal static void SaveCallsSnapshot(DateTime date, int sent, int received)
        {
            SaveCallsSnapshot(date, sent, received, false);
        }

        /// <summary>
        /// Saves the timestamp, service, sent items count, received items count into the database
        /// </summary>
        /// <param name="window"></param>
        /// <param name="process"></param>
        /// <param name="date"></param>
        /// <param name="sent"></param>
        /// <param name="received"></param>
        internal static void SaveCallsSnapshot(DateTime date, int sent, int received, bool isFromTimer)
        {
            Database.GetInstance().ExecuteDefaultQuery("INSERT INTO " + Settings.CallsTable + " (timestamp, time, service, sent, received, isFromTimer) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                Database.GetInstance().QTime(date) + ", " + "'skype'" + ", " + Database.GetInstance().Q(sent) + ", " + Database.GetInstance().Q(received) + ", " + Database.GetInstance().Q(isFromTimer) + ");");
        }

        /// <summary>
        /// Check if there is already calls (with isFromTimer = 0) stored for the date
        /// </summary>
        /// <returns>true if yes, false otherwise</returns>
        internal static bool HasCallsEntriesForDate(DateTimeOffset date, bool isFromTimer)
        {
            try
            {
                var query = "SELECT EXISTS( "
                          + "SELECT 1 FROM " + Settings.CallsTable + " "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                          + "AND isFromTimer = " + Database.GetInstance().Q(isFromTimer) + ");";

                var count = Database.GetInstance().ExecuteScalar(query);
                if (count == 0) return false;
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return false;
            }
        }

        /// <summary>
        /// The calls sent or received
        /// </summary>
        /// <param name="date"></param>
        /// <param name="sentOrReceived"></param>
        /// <returns>Tuple item1: sent, item2: received</returns>
        internal static Tuple<DateTime, int> GetSentOrReceivedCalls(DateTimeOffset date, string sentOrReceived)
        {
            try
            {
                var query = "SELECT time, " + sentOrReceived + " FROM " + Settings.CallsTable + " "
                            + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) + " "
                            + "ORDER BY time DESC "
                            + "LIMIT 1;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                if (table != null && table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    var answer = Convert.ToInt32(row[sentOrReceived], CultureInfo.InvariantCulture);
                    var timestamp = DateTime.Parse((string)row["time"], CultureInfo.InvariantCulture);

                    table.Dispose();
                    return new Tuple<DateTime, int>(timestamp, answer);
                }
                else
                {
                    table.Dispose();
                    return new Tuple<DateTime, int>(DateTime.MinValue, -1);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
                return new Tuple<DateTime, int>(DateTime.MinValue, -1);
            }
        }
    }
}
