using System;
using Shared;
using Shared.Data;
using System.Collections.Generic;
using MuseTracker.Models;
using System.Globalization;
using System.Data;

namespace MuseTracker.Data
{
    public class Queries
    {
        internal static void CreateMuseInputTables()
        {
            try
            {
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseEEGData + " (id INTEGER PRIMARY KEY, eegType TEXT, avg REAL, channelLeft REAL, channelFrontLeft REAL, channelFrontRight REAL, channelRight REAL, time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseBlink + " (id INTEGER PRIMARY KEY, blink INTEGER, time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseConcentration + " (id INTEGER PRIMARY KEY, concentration REAL, time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseMellow + " (id INTEGER PRIMARY KEY, mellow REAL, time TEXT, timestamp TEXT)");
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
                        query = "INSERT INTO '" + Settings.DbTableMuseEEGData + "' (eegType, avg, channelLeft, channelFrontLeft, channelFrontRight, channelRight, time, timestamp) VALUES ";
                        newQuery = false;
                    }
                    else
                    {
                        query += ", ";
                    }

                    query += "(" + 
                             Database.GetInstance().Q((item).DataType.ToString()) + "," +
                             Database.GetInstance().Q((item).Avg.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).ChannelLeft.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).ChannelFrontLeft.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).ChannelFrontRight.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).ChannelRight.ToString(CultureInfo.InvariantCulture)) + "," +
                             "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) + 
                             ")";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        query += ";";
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    query += ";";
                    Database.GetInstance().ExecuteDefaultQuery(query);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("## Error In MuseEEGData");
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
                        query = "INSERT INTO '" + Settings.DbTableMuseBlink + "' (blink, time, timestamp) VALUES";
                        newQuery = false;
                    }
                    else
                    {
                        query += ", ";
                    }

                    query += "(" + 
                            Database.GetInstance().Q((item).Blink.ToString()) + "," +
                             "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) +
                             ")";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        query += ";";
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    query += ";";
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
                        query = "INSERT INTO '" + Settings.DbTableMuseConcentration + "' (concentration, time, timestamp) VALUES";
                        newQuery = false;
                    }
                    else
                    {
                        query += ", ";
                    }

                    query += "(" + 
                             Database.GetInstance().Q((item).Concentration.ToString()) + "," +
                             "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) +
                             ")";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        query += ";";
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    query += ";";
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
                        query = "INSERT INTO '" + Settings.DbTableMuseMellow + "' (mellow, time, timestamp) VALUES";
                        newQuery = false;
                    }
                    else
                    {
                        query += ", ";
                    }

                    query += "(" + 
                             Database.GetInstance().Q((item).Mellow.ToString()) + "," +
                             "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime'), " +
                             Database.GetInstance().QTime(item.Timestamp) +
                             ")";

                    //executing remaining lines
                    if (i != 0 && i % 499 == 0)
                    {
                        query += ";";
                        Database.GetInstance().ExecuteDefaultQuery(query);
                        newQuery = true;
                        query = string.Empty;
                    }
                }

                //executing remaining lines
                if (i % 499 != 0)
                {
                    query += ";";
                    Database.GetInstance().ExecuteDefaultQuery(query);
                }
            }
            catch (Exception e)
            {
                Shared.Logger.WriteToLogFile(e);
            }
        }


        /// <summary>
        /// Fetches the eze blinks of a user for a given date and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetBlinks(DateTimeOffset date)
        {
            var dto = new Dictionary<string, int>();

            try
            {
                var query = "SELECT time, count(blink)" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                            " GROUP BY time;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timestamp = (string)row[0];
                    var blinkCounter = 0;
                    int.TryParse(row[1].ToString(), out blinkCounter);

                    if (dto.ContainsKey(timestamp))
                    {
                        dto[timestamp] += blinkCounter;
                    }
                    else
                    {
                        dto.Add(timestamp, blinkCounter);
                    }
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return dto;
        }
    }
}
