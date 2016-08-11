using System;
using Shared;
using Shared.Data;
using System.Collections.Generic;
using MuseTracker.Models;

namespace MuseTracker.Data
{
    public class Queries
    {
        internal static void CreateMuseInputTables()
        {
            try
            {
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseEEGData + " (id INTEGER PRIMARY KEY, eegType TEXT, avg FLOAT(8,7), channelLeft FLOAT(8,7), channelFrontLeft FLOAT(8,7), channelFrontRight FLOAT(8,7), channelRight FLOAT(8,7), time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseBlink + " (id INTEGER PRIMARY KEY, blink INTEGER, time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseConcentration + " (id INTEGER PRIMARY KEY, concentration FLOAT(8,7), time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseMellow + " (id INTEGER PRIMARY KEY, mellow FLOAT(8,7), time TEXT, timestamp TEXT)");
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        internal static void SaveMuseEEGDataToDatabase(IReadOnlyList<IMuseTrackerInput> eegData)
        {
            try
            {
                if (eegData == null || eegData.Count == 0) return;
                var newQuery = true;
                var query = "";
                int i;
                for (i = 0; i < eegData.Count; i++)
                {
                    var item = (MuseEEGDataEvent)eegData[i];
                    if (item == null) continue;

                    if (newQuery)
                    {
                        query = "INSERT INTO '" + Settings.DbTableMuseEEGData + "' (eegType, avg, channelLeft, channelFrontLeft, channelFrontRight, channelRight, time, timestamp) ";
                        newQuery = false;
                    }
                    else
                    {
                        query += "UNION ALL ";
                    }

                    query += Database.GetInstance().Q((item).DataType.ToString()) + "," +
                             Database.GetInstance().Q((item).Avg.ToString()) + "," +
                             Database.GetInstance().Q((item).ChannelLeft.ToString()) + "," +
                             Database.GetInstance().Q((item).ChannelFrontLeft.ToString()) + "," +
                             Database.GetInstance().Q((item).ChannelFrontRight.ToString()) + "," +
                             Database.GetInstance().Q((item).ChannelRight.ToString()) + "," +
                             "SELECT strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) + " ";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    Database.GetInstance().ExecuteDefaultQuery(query);
                }
            }
            catch (Exception e)
            {
                Shared.Logger.WriteToLogFile(e);
            }
        }


        internal static void SaveMuseBlinksToDatabase(IReadOnlyList<IMuseTrackerInput> blinks)
        {
            try
            {
                if (blinks == null || blinks.Count == 0) return;
                var newQuery = true;
                var query = "";
                int i;

                for (i = 0; i < blinks.Count; i++)
                {
                    var item = (MuseBlinkEvent) blinks[i];
                    if (item == null) continue;

                    if (newQuery)
                    {
                        query = "INSERT INTO '" + Settings.DbTableMuseBlink + "' (blink, time, timestamp) ";
                        newQuery = false;
                    }
                    else
                    {
                        query += "UNION ALL ";
                    }

                    query += Database.GetInstance().Q((item).Blink.ToString()) + "," +
                             "SELECT strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) + " ";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    Database.GetInstance().ExecuteDefaultQuery(query);
                }
            }
            catch (Exception e)
            {
                Shared.Logger.WriteToLogFile(e);
            }
        }

        internal static void SaveMuseConcentrationToDatabase(IReadOnlyList<IMuseTrackerInput> concentrationData)
        {
            try
            {
                if (concentrationData == null || concentrationData.Count == 0) return;
                var newQuery = true;
                var query = "";
                int i;

                for (i = 0; i < concentrationData.Count; i++)
                {
                    var item = (MuseConcentrationEvent) concentrationData[i];
                    if (item == null) continue;

                    if (newQuery)
                    {
                        query = "INSERT INTO '" + Settings.DbTableMuseConcentration + "' (concentration, time, timestamp) ";
                        newQuery = false;
                    }
                    else
                    {
                        query += "UNION ALL ";
                    }

                    query += Database.GetInstance().Q((item).Concentration.ToString()) + "," +
                             "SELECT strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) + " ";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    Database.GetInstance().ExecuteDefaultQuery(query);
                }
            }
            catch (Exception e)
            {
                Shared.Logger.WriteToLogFile(e);
            }
        }

        internal static void SaveMuseMellowToDatabase(IReadOnlyList<IMuseTrackerInput> mellowData)
        {
            try
            {
                if (mellowData == null || mellowData.Count == 0) return;
                var newQuery = true;
                var query = "";
                int i;

                for (i = 0; i < mellowData.Count; i++)
                {
                    var item = (MuseMellowEvent)mellowData[i];
                    if (item == null) continue;

                    if (newQuery)
                    {
                        query = "INSERT INTO '" + Settings.DbTableMuseMellow + "' (mellow, time, timestamp) ";
                        newQuery = false;
                    }
                    else
                    {
                        query += "UNION ALL ";
                    }

                    query += Database.GetInstance().Q((item).Mellow.ToString()) + "," +
                             "SELECT strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) + " ";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    Database.GetInstance().ExecuteDefaultQuery(query);
                }
            }
            catch (Exception e)
            {
                Shared.Logger.WriteToLogFile(e);
            }
        }

    }
}
