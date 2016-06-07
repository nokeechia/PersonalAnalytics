// Created by André Meyer from UZH
// Created: 2016-05-11
// 
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shared.Data.Extractors;
using Shared.Data;
using InteractionTracker;
using System.IO;
using InteractionTracker.Data;
using System.Linq;

namespace InteractionTrackerTests
{
    /// <summary>
    /// This test suite tests the InteractionTracker. 
    /// Important! Always call database instead of GetInstance() as we don't
    /// want to test on the live database file!
    /// </summary>
    [TestClass]
    public class QueriesTest
    {
        private DatabaseImplementation _database;

        [TestInitialize]
        public void Setup()
        {
            Debug.WriteLine("# TestSuite QueriesTest started.");

            // set up testing database
            _database = Database.GetInstanceTesting();
            _database.Connect();
        }

        [TestMethod]
        public void GetFocusTimeInLastHourTest()
        {
            // TODO: test
        }

        [TestMethod]
        public void GetNumInteractionSwitchesTest()
        {
            // add table & dummy data
            _database.ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Shared.Settings.WindowsActivityTable + " (id INTEGER PRIMARY KEY, time TEXT, window TEXT, process TEXT)");
            
            var activities = new List<Tuple<DateTime, string>>();
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-1), "skype"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-4), "outlook"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-10), "other"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-11), "other"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-16), "skype"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-45), "other"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-54), "outlook"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-56), "other"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-61), "other"));
            activities.Add(new Tuple<DateTime, string>(DateTime.Now.AddMinutes(-76), "outlook"));

            foreach (var activity in activities.OrderBy(a => a.Item1))
            {
                _database.ExecuteDefaultQuery("INSERT INTO " + Shared.Settings.WindowsActivityTable + " (time, process) VALUES (" +
                    _database.QTime(activity.Item1) + ", " + _database.Q(activity.Item2) + ");");
            }

            // run tests
            var numActivitySwitches = Queries.GetNumInteractionSwitches();
            Assert.AreEqual(6, numActivitySwitches);

            // clean-up
            _database.ExecuteDefaultQuery("DROP TABLE " + Shared.Settings.WindowsActivityTable + ";");

        }

        [TestMethod]
        public void GetMeetingsForLastHourTest()
        {
            // add table & dummy data
            _database.ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Shared.Settings.MeetingsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, subject TEXT, durationInMins INTEGER)");

            var meetings = new List<Tuple<DateTime, int>>();
            meetings.Add(new Tuple<DateTime, int>(DateTime.Now.AddMinutes(20), 20));
            meetings.Add(new Tuple<DateTime, int>(DateTime.Now.AddSeconds(-10), 30));
            meetings.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(-20), 40));
            meetings.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(2), 50));
            meetings.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(1), 60));
            meetings.Add(new Tuple<DateTime, int>(DateTime.Now.AddMinutes(-59), 70));

            foreach (var meeting in meetings)
            {
                _database.ExecuteDefaultQuery("INSERT INTO " + Shared.Settings.MeetingsTable + " (timestamp, time, durationInMins) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                    _database.QTime(meeting.Item1) + ", " + _database.Q(meeting.Item2) + ");");
            }

            // run tests
            var numMeetingsLastHour = Queries.GetNumMeetingsForLastHour();

            Assert.AreEqual(2, numMeetingsLastHour);

            // clean-up
            _database.ExecuteDefaultQuery("DROP TABLE " + Shared.Settings.MeetingsTable + ";");
        }

        [TestMethod]
        public void GetMeetingsForDateTest()
        {
            // add table & dummy data
            _database.ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Shared.Settings.MeetingsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, subject TEXT, durationInMins INTEGER)");

            var meetings = new List<Tuple<DateTime, string>>();
            meetings.Add(new Tuple<DateTime, string>(DateTime.Now, "Meeting 1"));
            meetings.Add(new Tuple<DateTime, string>(DateTime.Now.AddSeconds(-10), "Meeting 2"));
            meetings.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(-20), "Meeting 3"));
            meetings.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(2), "Meeting 4"));
            meetings.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(1), "Meeting 5"));

            foreach (var meeting in meetings)
            {
                _database.ExecuteDefaultQuery("INSERT INTO " + Shared.Settings.MeetingsTable + " (timestamp, time, subject) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                    _database.QTime(meeting.Item1) + ", " + _database.Q(meeting.Item2) + ");");
            }

            // run tests
            var totalNumMeetingsYesterday = Queries.GetNumMeetingsForDate(DateTime.Now.AddDays(-1));
            var totalNumMeetingsToday = Queries.GetNumMeetingsForDate(DateTime.Now);
            var totalNumMeetingsTomorrow = Queries.GetNumMeetingsForDate(DateTime.Now.AddDays(1));

            Assert.AreEqual(0, totalNumMeetingsYesterday);
            Assert.AreEqual(2, totalNumMeetingsToday);
            Assert.AreEqual(1, totalNumMeetingsTomorrow);

            // clean-up
            _database.ExecuteDefaultQuery("DROP TABLE " + Shared.Settings.MeetingsTable + ";");
        }

        [TestMethod]
        public void GetSentOrReceivedEmailsTest()
        {
            // add table & dummy data
            _database.ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Shared.Settings.EmailsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, inbox INTEGER, sent INTEGER, received INTEGER, isFromTimer INTEGER)");

            var emails = new List<Tuple<DateTime, int, int>>();
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now, 10, 1));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddHours(-10), 20, 2));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddDays(-20), 30, 3));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddDays(2), 40, 4));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddSeconds(-10), 50, 5));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddDays(-1), 60, 6));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddDays(-1).AddSeconds(20), 70, 7));
            emails.Add(new Tuple<DateTime, int, int>(DateTime.Now.AddDays(-1).AddMinutes(-10), 10, 1));


            foreach (var email in emails)
            {
                _database.ExecuteDefaultQuery("INSERT INTO " + Shared.Settings.EmailsTable + " (timestamp, time, sent, received) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                    _database.QTime(email.Item1) + ", " + _database.Q(email.Item2) + ", " + _database.Q(email.Item3) + ");");
            }

            // run tests
            var totalEmailsSentToday = Queries.GetSentOrReceivedEmails(DateTime.Now.Date, "sent");
            var totalEmailsReceivedToday = Queries.GetSentOrReceivedEmails(DateTime.Now.Date, "received");
            Assert.AreEqual(10, totalEmailsSentToday);
            Assert.AreEqual(1, totalEmailsReceivedToday);

            var totalEmailsSentYesterday = Queries.GetSentOrReceivedEmails(DateTime.Now.Date.AddDays(-1), "sent");
            var totalEmailsReceivedYesterday = Queries.GetSentOrReceivedEmails(DateTime.Now.Date.AddDays(-1), "received");
            Assert.AreEqual(70, totalEmailsSentYesterday);
            Assert.AreEqual(7, totalEmailsReceivedYesterday);

            var totalEmailsSentOtherDay = Queries.GetSentOrReceivedEmails(DateTime.Now.Date.AddDays(-100), "sent");
            var totalEmailsReceivedOtherDay = Queries.GetSentOrReceivedEmails(DateTime.Now.Date.AddDays(-100), "received");
            Assert.AreEqual(-1, totalEmailsSentOtherDay);
            Assert.AreEqual(-1, totalEmailsReceivedOtherDay);


            // clean-up
            _database.ExecuteDefaultQuery("DROP TABLE " + Shared.Settings.EmailsTable + ";");
        }

        /// <summary>
        /// Hint: currently, we only check the chats
        /// </summary>
        [TestMethod]
        public void GetCallsOrChatsTest()
        {
            // add table & dummy data
            _database.ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Shared.Settings.ChatsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, service TEXT, sent INTEGER, received INTEGER, isFromTimer INTEGER)");
            //_database.ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.CallsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, service TEXT, sent INTEGER, received INTEGER, isFromTimer INTEGER)");

            var chats = new List<Tuple<DateTime, int>>();
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddMinutes(20), 3));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddSeconds(-10), 3));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(-20), 2));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(2), 3));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(-1), 2));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(-1).AddSeconds(33), 3));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddDays(-1), 1));
            chats.Add(new Tuple<DateTime, int>(DateTime.Now.AddMinutes(-59), 0));

            foreach (var chat in chats)
            {
                _database.ExecuteDefaultQuery("INSERT INTO " + Shared.Settings.ChatsTable + " (timestamp, time, sent, received) VALUES (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), " +
                    _database.QTime(chat.Item1) + ", " + chat.Item2 + ", 0);");
            }

            // run tests
            var numChatsToday = Queries.GetNumCallsOrChats(DateTime.Now.Date, Shared.Settings.ChatsTable);
            var numChatsYesterday = Queries.GetNumCallsOrChats(DateTime.Now.AddDays(-1).Date, Shared.Settings.ChatsTable);
            var numChatsOtherDay = Queries.GetNumCallsOrChats(DateTime.Now.AddDays(-100).Date, Shared.Settings.ChatsTable);

            Assert.AreEqual(3, numChatsToday);
            Assert.AreEqual(3, numChatsYesterday);
            Assert.AreEqual(-1, numChatsOtherDay); // -1 means: we don't know, we have no entries

            // clean-up
            _database.ExecuteDefaultQuery("DROP TABLE " + Shared.Settings.ChatsTable + ";");
            //_database.ExecuteDefaultQuery("DROP TABLE " + Settings.CallsTable + ";");
        }

        [TestCleanup]
        public void Teardown()
        {
            // disconnect from database
            _database.Disconnect();

            // delete testing database
            var testDbFile = Database.GetTestingDatabaseSavePath();
            if (File.Exists(testDbFile)) File.Delete(testDbFile);

            Debug.WriteLine("# TestSuite QueriesTest completed.");
        }
    }
}
