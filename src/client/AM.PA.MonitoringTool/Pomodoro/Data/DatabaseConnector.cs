using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Data;
using Shared;
using System.Data;
using System.Globalization;



namespace Pomodoro
{
    public class DatabaseConnector
    {
        //Database Fields for Pomodoro Table

        private const string ID = "id";

        //is set when the pomodoro is started
        private const string StartTime = "startTime";

        //set when the pomodoro is completed
        private const string EndTime = "endTime";

        //duration of the actual pomodoro (without pausing) 
        //(this is there because the duration can be changed in the settings, to enable calculating the total minutes worked)
        private const string Duration = "duration";

        //paused:<DateTime>,resumed:<DateTime>, ... 
        //(this is there to visualize pauses in a pomodoro timeline)
        private const string PausedResumed = "pausedResumed";

        //task that the pomodoro was spent on
        private const string Task = "task"; 

        //CREATE Pomodoros
        private static readonly string CREATE_POMODOROS_TABLE = "CREATE TABLE IF NOT EXISTS " + Settings.PomodoroTable + " ("
            + ID + " INTEGER PRIMARY KEY, "
            + StartTime + " DATETIME, "
            + EndTime + " DATETIME, "
            + Duration + " INTEGER, " 
            + PausedResumed + " TEXT, " 
            + Task + " TEXT);";

        //INSERT Pomodoro
        private static readonly string INSERT_POMODORO_QUERY = "INSRET INTO " + Settings.PomodoroTable 
            + "(" + StartTime + ", " 
            + EndTime + ", " 
            + Duration + ", " 
            + PausedResumed + ", "
            + Task + ") VALUES ("
            + "{0}, "
            + "{1}, "
            + "{2}, "
            + "{3}, "
            + "{4});";

        //SELECT Pomodoro of a certain day
        private static readonly string SELECT_POMODOROS_QUERY = "SELECT * FROM " + Settings.PomodoroTable
            + "WHERE strftime('" + Settings.DateDayFormat + "', " + EndTime + ") = {0}";
            

        #region Create

        /// <summary>
        /// Creates all the tables that are needed for this tracker if the tables do not exist
        /// </summary>
        internal static void CreatePomodorosTableIfNotExists()
        {
            try
            {
                Database.GetInstance().ExecuteDefaultQuery(CREATE_POMODOROS_TABLE);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds a new pomodoro to the database
        /// </summary>
        /// <param name="Pomodoro"></param>
        internal static void AddPomodoro(Pomodoro pomodoro)
        {
            try
            {
                string query = String.Format(INSERT_POMODORO_QUERY, 
                    pomodoro.StartTime.ToString(Settings.DateFormat), 
                    pomodoro.EndTime.ToString(Settings.DateFormat),
                    pomodoro.Duration.ToString(),
                    GetPausedResumedString(pomodoro.PausedResumed),
                    pomodoro.Task);
                
                Database.GetInstance().ExecuteDefaultQuery(query);
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        internal static string GetPausedResumedString(Dictionary<string, DateTime> pausedResumedDict)
        {
            string pausedResumedString = string.Empty;

            foreach(KeyValuePair<string, DateTime> pausedResumedEvent in pausedResumedDict)
            {
                pausedResumedString += pausedResumedEvent.Key + ":" + pausedResumedEvent.Value.ToString(Settings.DateFormat) + ",";
            }

            return pausedResumedString;
        }

        #endregion

        #region Select

        internal static List<Pomodoro> GetPomodorosOfDay(DateTime date)
        {
            string query = String.Format(SELECT_POMODOROS_QUERY, date.ToString(Settings.DateDayFormat));
            
            var pomodoros = new List<Pomodoro>();
            var table = Database.GetInstance().ExecuteReadQuery(SELECT_POMODOROS_QUERY);

            try
            {
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        pomodoros.Add(new Pomodoro
                        {
                            ID = int.Parse(row[ID].ToString()),
                            StartTime = DateTime.ParseExact(row[StartTime].ToString(), Settings.DateFormat, CultureInfo.InvariantCulture),
                            EndTime = DateTime.ParseExact(row[EndTime].ToString(), Settings.DateFormat, CultureInfo.InvariantCulture),
                            Duration = int.Parse(row[Duration].ToString()),
                            PausedResumed = GetPausedResumedDict(row[PausedResumed].ToString()),
                            Task = row[Task].ToString()
                        });
                    }
                }
                else
                {
                    table.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
            finally
            {
                table.Dispose();
            }

            return pomodoros;
        }

        internal static Dictionary<string, DateTime> GetPausedResumedDict(string pausedResumedString)
        {
            Dictionary<string, DateTime> pausedResumedDict = new Dictionary<string, DateTime>();

            foreach (string pausedResumedEventString in pausedResumedString.Split(','))
            {
                if (pausedResumedEventString.Length > 0)
                {
                    string[] pausedResumedStrArr = pausedResumedEventString.Split(':');
                    pausedResumedDict.Add(pausedResumedStrArr[0], DateTime.ParseExact(pausedResumedStrArr[1], Settings.DateFormat, CultureInfo.InvariantCulture));
                }
            }

            return pausedResumedDict;
        }

        #endregion
    }
}
