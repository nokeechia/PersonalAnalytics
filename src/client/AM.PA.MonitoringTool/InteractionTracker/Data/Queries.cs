// Created by André Meyer at UZH and Paige Rodeghero
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
        /// <param name="date"></param>
        /// <returns></returns>
        internal static int GetFocusTimeInLastHour(DateTimeOffset date)
        {
            try
            {
                var query = "SELECT 3600 - SUM(difference) as 'difference'"
                          + "FROM ( "
                          + "SELECT (strftime('%s', t2.time) - strftime('%s', t1.time)) as 'difference' " 
                          + "FROM " + Settings.WindowsActivityTable + " t1 LEFT JOIN " + Settings.WindowsActivityTable + " t2 on t1.id + 1 = t2.id "
                          + "WHERE " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "t1.time") + " and " + Database.GetInstance().GetDateFilteringStringForQuery(VisType.Day, date, "t2.time") + " "
                          + "AND TIME(t2.time) between TIME('" + DateTime.Now.AddHours(-1).ToString("HH:mm") + "') and TIME('"+DateTime.Now.ToString("HH:mm") +"') "
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
    }
}
