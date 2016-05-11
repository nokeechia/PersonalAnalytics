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

namespace InteractionTrackerTests
{
    /// <summary>
    /// This test suite tests the InteractionTracker. 
    /// Important! Always call Database.GetInstanceTesting() instead of GetInstance() as we don't
    /// want to test on the live database file!
    /// </summary>
    [TestClass]
    public class QueriesTest
    {
        [TestInitialize]
        public void Setup()
        {
            Debug.WriteLine("# TestSuite QueriesTest started.");

            // set up testing database
            Database.GetInstanceTesting().Connect();
        }

        [TestMethod]
        public void GetFocusTimeInLastHourTest()
        {
            // TODO: test
        }

        [TestMethod]
        public void GetNoInteractionSwitchesTest()
        {
            // TODO: test
        }

        [TestMethod]
        public void GetMeetingsForLastHourTest()
        {
            // add table & dummy data
            Database.GetInstanceTesting().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.MeetingsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, subject TEXT, durationInMins INTEGER)");

            // run tests

        }

        [TestMethod]
        public void GetMeetingsForDateTest()
        {
            // add table & dummy data
            Database.GetInstanceTesting().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.MeetingsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, subject TEXT, durationInMins INTEGER)");

            // run tests

        }

        [TestMethod]
        public void GetSentOrReceivedEmailsTest()
        {
            // add table & dummy data
            Database.GetInstanceTesting().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.EmailsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, inbox INTEGER, sent INTEGER, received INTEGER, isFromTimer INTEGER)");

            // run tests

        }

        [TestMethod]
        public void GetCallsOrChatsTest()
        {
            // add table & dummy data
            Database.GetInstanceTesting().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.ChatsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, service TEXT, sent INTEGER, received INTEGER, isFromTimer INTEGER)");
            Database.GetInstanceTesting().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.CallsTable + " (id INTEGER PRIMARY KEY, timestamp TEXT, time TEXT, service TEXT, sent INTEGER, received INTEGER, isFromTimer INTEGER)");

            // run tests

        }

        [TestCleanup]
        public void Teardown()
        {
            // disconnect from database
            Database.GetInstanceTesting().Disconnect();
            Database.GetInstanceTesting().Dispose();

            // delete testing database
            var testDbFile = Database.GetTestingDatabaseSavePath();
            if (File.Exists(testDbFile)) File.Delete(testDbFile);

            Debug.WriteLine("# TestSuite QueriesTest completed.");
        }
    }
}
