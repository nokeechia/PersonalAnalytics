using System;
using Shared;
using Shared.Data;
using System.Collections.Generic;
using MuseTracker.Models;
using System.Globalization;
using System.Data;
using System.Linq;
using UserEfficiencyTracker.Data;

namespace MuseTracker.Data
{
    public class Queries
    {
        internal static void CreateMuseInputTables()
        {
            try
            {
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseEEGData + " (id INTEGER PRIMARY KEY, eegType TEXT, avg REAL, channelLeft REAL, channelFrontLeft REAL, channelFrontRight REAL, channelRight REAL, time TEXT, timestamp TEXT)");
                Database.GetInstance().ExecuteDefaultQuery("CREATE TABLE IF NOT EXISTS " + Settings.DbTableMuseEEGDataQuality + " (id INTEGER PRIMARY KEY, qualityChannelLeft INTEGER, qualityChannelFrontLeft INTEGER, qualityChannelFrontRight INTEGER, qualityChannelRight INTEGER, time TEXT, timestamp TEXT)");
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


        internal static void SaveMuseEEGDataQualityToDatabase(IReadOnlyList<IMuseTrackerInput> eegData)
        {
            try
            {
                if (eegData == null || eegData.Count == 0) return;
                var newQuery = true;
                var query = "";
                int i;
                for (i = 0; i < eegData.Count; i++)
                {
                    var item = (MuseEEGDataQuality)eegData[i];
                    if (item == null) continue;

                    if (newQuery)
                    {
                        query = "INSERT INTO '" + Settings.DbTableMuseEEGDataQuality + "' (qualityChannelLeft, qualityChannelFrontLeft, qualityChannelFrontRight, qualityChannelRight, time, timestamp) VALUES ";
                        newQuery = false;
                    }
                    else
                    {
                        query += ", ";
                    }

                    query += "(" +
                             Database.GetInstance().Q((item).QualityChannelLeft.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).QualityChannelFrontLeft.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).QualityChannelFrontRight.ToString(CultureInfo.InvariantCulture)) + "," +
                             Database.GetInstance().Q((item).QualityChannelRight.ToString(CultureInfo.InvariantCulture)) + "," +
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
                Console.WriteLine("## Error In MuseEEGDataQuality");
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
        /// Fetches eye blinks of a user for a given date and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, int>> GetBlinks(DateTimeOffset date)
        {
            var resList = new List<Tuple<DateTime, int>>();

            try
            {
                var query = "SELECT time, sum(blink)" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                            " GROUP BY time;";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timestamp = (String)row[0];
                    var blinkCounter = 0;
                    int.TryParse(row[1].ToString(), out blinkCounter);

                    resList.Add(new Tuple<DateTime, int>(DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), blinkCounter));
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return resList;
        }

        /// <summary>
        /// Fetches average eye blinks of a user for a given date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double GetAvgBlinksByDate(DateTimeOffset date)
        {
            var avg = 0.0;

            try
            {
                var query = "SELECT avg(blink) as avgblinks" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) +
                            ";";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    avg = Convert.ToDouble(row["avgblinks"]);
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return avg;
        }

        /// <summary>
        /// Fetches average eye blinks of a user for a given time range
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double GetBlinksWithinTimerange(DateTime date, List<TopProgramTimeDto> dtoList)
        {
            var avg = 0.0;

            try
            {
                var query = "SELECT avg(blink) as avgblinks" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date) +
                            " AND (" + buildORClause(dtoList) + ")" +
                            ";";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    avg = Convert.ToDouble(row["avgblinks"]);
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return avg;
        }

        /// <summary>
        /// Fetches eye blinks of a user for a given date and prepares the data from current week
        /// to be visualized.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, int>> GetBlinksOfWeek(DateTimeOffset date)
        {
            var resList = new List<Tuple<DateTime, int>>();

            try
            {
                var query = "SELECT strftime('%Y-%m-%d',time), sum(blink)" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Week, date, "time") +
                            " GROUP BY strftime('%Y-%m-%d',time);";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timestamp = (String)row[0];
                    var blinkCounter = 0;
                    int.TryParse(row[1].ToString(), out blinkCounter);

                    resList.Add(new Tuple<DateTime, int>(DateTime.ParseExact(timestamp, "yyyy-MM-dd", CultureInfo.InvariantCulture), blinkCounter));
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return resList;
        }

        /// <summary>
        /// Fetches eye blinks of a user for a given date and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, int>> GetBlinksByYear(DateTimeOffset date)
        {
            var resList = new List<Tuple<DateTime, int>>();

            try
            {
                var query = "SELECT strftime('%Y-%m-%d',time), sum(blink)" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Month, date, "time") +
                            " GROUP BY strftime('%Y-%m-%d',time);";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timestamp = (String)row[0];
                    var blinkCounter = 0;
                    int.TryParse(row[1].ToString(), out blinkCounter);

                    resList.Add(new Tuple<DateTime, int>(DateTime.ParseExact(timestamp, "yyyy-MM-dd", CultureInfo.InvariantCulture), blinkCounter));
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return resList;
        }

        /// <summary>
        /// Fetches eye blinks of a user for a given date and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, int>> GetBlinksGroupedByHour(DateTimeOffset date)
        {
            var resList = new List<Tuple<DateTime, int>>();

            try
            {
                var query = "SELECT strftime('%Y-%m-%d %H',time), sum(blink)" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                            " GROUP BY strftime('%Y-%m-%d %H',time);";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timestamp = (String)row[0];
                    var blinkCounter = 0;
                    int.TryParse(row[1].ToString(), out blinkCounter);
                    string tsdExtended = timestamp + ":00:00";
                    resList.Add(new Tuple<DateTime, int>(DateTime.ParseExact(tsdExtended, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), blinkCounter));
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return resList;
        }

        /// <summary>
        /// Fetches eye blinks of a user for a given date aggregated by a certain minutes interval and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, int>> GetBlinksByMinutesInterval(DateTimeOffset date, int intervalInMinutes)
        {
            var resList = new List<Tuple<DateTime, int>>();

            try
            {
                var query = "SELECT strftime('%Y-%m-%d %H:%M:%S',time), sum(blink)" +
                            " FROM " + Settings.DbTableMuseBlink +
                            " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                            " GROUP BY strftime('%Y-%m-%d %H',time), strftime('%M', time)/" + intervalInMinutes + ";";

                var table = Database.GetInstance().ExecuteReadQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    var timestamp = (String)row[0];
                    var blinkCounter = 0;
                    int.TryParse(row[1].ToString(), out blinkCounter);
                    resList.Add(new Tuple<DateTime, int>(DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), blinkCounter));
                }
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return resList;
        }

        /// <summary>
        /// Fetches eeg data of a user for a given date and prepares the data
        /// aggregated by date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double GetAvgEEGIndexByDate(DateTimeOffset date)
        {
            var eegIndices = GetEEGIndex(date);
            return eegIndices.Count > 0 ? eegIndices.Average(i => i.Item2) : 0.0;            
        }


        /// <summary>
        /// Fetches eeg data of a user between given from and to dates.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, double>> GetEEGIndexWithinTimerange(DateTime date, List<TopProgramTimeDto> dtoList)
        {
            var query = "SELECT time, eegType, avg(avg)" +
                        " FROM " + Settings.DbTableMuseEEGData +
                        " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                        " AND (" + buildORClause(dtoList) + ")" +
                        " GROUP BY time, eegType;";

            return ExecuteInstructions(query);
        }

        private static String buildORClause(List<TopProgramTimeDto> dtos)
        {
            var clause = "";
            foreach (TopProgramTimeDto dto in dtos)
            {
                clause += Database.GetInstance().GetDateFilteringStringForQuery(dto.From, dto.To, "time");
                clause += " OR ";
            }
            return clause.Substring(0, clause.Length-3); // remove last OR
        }
        private static List<Tuple<DateTime, double>> ExecuteInstructions(String query)
        {
            var result = new List<Tuple<DateTime, double>>();

            try
            {

                var table = Database.GetInstance().ExecuteReadQuery(query);
                result = FromDictToList(FromTableToDict(table));
                table.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return result;
        }

        private static Dictionary<string, Tuple<double, double, double>> FromTableToDict(DataTable table)
        {
            var tempDict = new Dictionary<string, Tuple<double, double, double>>();
            foreach (DataRow row in table.Rows)
            {
                var timestamp = (String)row[0];
                if (tempDict.ContainsKey(timestamp))
                {
                    Tuple<double, double, double> values;
                    tempDict.TryGetValue(timestamp, out values);
                    var val = 0.0;
                    double.TryParse(row[2].ToString(), out val);

                    if ((String)row[1] == "AlphaAbsolute")
                    {
                        tempDict[timestamp] = new Tuple<double, double, double>(val, values.Item2, values.Item3);
                    }
                    if ((String)row[1] == "BetaAbsolute")
                    {
                        tempDict[timestamp] = new Tuple<double, double, double>(values.Item1, val, values.Item3);
                    }

                    if ((String)row[1] == "ThetaAbsolute")
                    {
                        tempDict[timestamp] = new Tuple<double, double, double>(values.Item1, values.Item2, val);
                    }

                }
                else
                {
                    var val = 0.0;
                    double.TryParse(row[2].ToString(), out val);
                    if ((String)row[1] == "AlphaAbsolute")
                    {
                        tempDict.Add(timestamp, new Tuple<double, double, double>(val, 0.0, 0.0));
                    }
                    if ((String)row[1] == "BetaAbsolute")
                    {
                        tempDict.Add(timestamp, new Tuple<double, double, double>(0.0, val, 0.0));
                    }
                    if ((String)row[1] == "ThetaAbsolute")
                    {
                        tempDict.Add(timestamp, new Tuple<double, double, double>(0.0, 0.0, val));
                    }
                }
            }
            return tempDict;
        }

        private static List<Tuple<DateTime, double>> FromDictToList(Dictionary<string, Tuple<double, double, double>> dict)
        {
            var resList = new List<Tuple<DateTime, double>>();
            foreach (KeyValuePair<string, Tuple<double, double, double>> entry in dict)
            {
                Tuple<double, double, double> tempValues = entry.Value;
                double eegIndex = computeEEGIndex(tempValues.Item2, tempValues.Item1, tempValues.Item3); //eeg index formula
                var tsd = "";

                //format string according to later parsing
                if (entry.Key.Length == ("yyyy-MM-dd HH").Length)
                {
                    tsd = entry.Key + ":00:00";
                }
                else
                {
                    tsd = entry.Key;
                }

                resList.Add(new Tuple<DateTime, double>(DateTime.ParseExact(tsd, getDateTimeFormat(tsd), CultureInfo.InvariantCulture), eegIndex));
            }
            return resList;

        }

        private static String getDateTimeFormat(String strDateTime)
        {
            if (strDateTime.Length == "yyyy-MM-dd".Length)
            {
                return "yyyy-MM-dd";
            }

            return "yyyy-MM-dd HH:mm:ss";
        }

        /// <summary>
        /// Fetches eeg data of a user for a given date and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, double>> GetEEGIndex(DateTimeOffset date)
        {
            var query = "SELECT time, eegType, avg(avg)" +
                        " FROM " + Settings.DbTableMuseEEGData +
                        " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                        " GROUP BY time, eegType;";

            return ExecuteInstructions(query);
        }

        /// <summary>
        /// Fetches eeg data of a user for a given date aggregated for a minute interval and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, double>> GetEEGIndexGroupedByMinutesInterval(DateTimeOffset date, int intervalInMinutes)
        {
            var query = "SELECT strftime('%Y-%m-%d %H:%M:%S',time), eegType, avg(avg)" +
                        " FROM " + Settings.DbTableMuseEEGData +
                        " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                        " GROUP BY strftime('%Y-%m-%d %H',time), strftime('%M', time)/" + intervalInMinutes +
                        ", eegType;";

            return ExecuteInstructions(query);
        }


        /// <summary>
        /// Fetches eye blinks of a user for a given date and prepares the data
        /// to be visualized as a line chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, double>> GetEEGIndexGroupedByHourADay(DateTimeOffset date)
        {
            var query = "SELECT strftime('%Y-%m-%d %H',time), eegType, avg(avg)" +
                        " FROM " + Settings.DbTableMuseEEGData +
                        " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "time") +
                        " GROUP BY strftime('%Y-%m-%d %H',time), eegType;";

            return ExecuteInstructions(query);
        }

        /// <summary>
        /// Fetches eeg activities of a user for current week and prepares the data
        /// to be visualized.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, double>> GetEEGIndexOfWeek(DateTimeOffset date)
        {
            var query = "SELECT strftime('%Y-%m-%d',time), eegType, avg(avg)" +
                        " FROM " + Settings.DbTableMuseEEGData +
                        " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Week, date, "time") +
                        " GROUP BY strftime('%Y-%m-%d',time), eegType;";

            return ExecuteInstructions(query);
        }

        /// <summary>
        /// Fetches eeg activities of a user for a year and prepares the data
        /// to be visualized as a contribution chart.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, double>> GetEEGIndexOfMonth(DateTimeOffset date)
        {
            var query = "SELECT strftime('%Y-%m-%d',time), eegType, avg(avg)" +
                        " FROM " + Settings.DbTableMuseEEGData +
                        " WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Month, date, "time") +
                        " GROUP BY strftime('%Y-%m-%d',time), eegType;";

            return ExecuteInstructions(query);
        }

        /// <summary>
        /// Computes EEG Index by receiving absolute (log scaled) values
        /// </summary>
        /// <param name="beta"></param>
        /// <param name="alpha"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        private static double computeEEGIndex(double beta, double alpha, double theta) {
            return Math.Pow(10, beta) / (Math.Pow(10, alpha) + Math.Pow(10, theta));
        }
    }
}
