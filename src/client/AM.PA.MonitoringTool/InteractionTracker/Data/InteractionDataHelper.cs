// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2016-06-06
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractionTracker.Data
{
    public class InteractionDataSet
    {
        public int NumMeetingsNow { get; set; }
        public int NumEmailsReceivedNow { get; set; }
        public int NumEmailsSentNow { get; set; }
        public int NumChatsNow { get; set; }

        public double AvgMeetingsPrevious { get; set; }
        public double AvgEmailsReceivedPrevious { get; set; }
        public double AvgEmailsSentPrevious { get; set; }
        public double AvgChatsPrevious { get; set; }

        public double MeetingsSD { get; set; }
        public double EmailsReceivedSD { get; set; }
        public double EmailsSentSD { get; set; }
        public double ChatsSD { get; set; }
    }

    public static class InteractionDataHelper
    {
        private static DateTime _lastTimeCalculatedDataSet; // caching
        private static InteractionDataSet _previousInteractionDataSet; // caching

        public static InteractionDataSet GetAllInteractionData(DateTimeOffset date, bool force = false)
        {
            // return cached data set if possible (speed!)
            if (force == false && // not forced
                _previousInteractionDataSet != null && // cached data set is available
                (DateTime.Now - _lastTimeCalculatedDataSet).TotalMinutes < 5) // it's recent 
            {
                return _previousInteractionDataSet;
            }

            // else, calculate it again
            try
            {
                var data = new InteractionDataSet();

                // get now data
                var now = date.Date.AddHours(18);
                data.NumMeetingsNow = Queries.GetMeetingsFromSixAm(now).Count;
                data.NumEmailsReceivedNow = Queries.GetEmailsSentOrReceivedFromSixAm(now, "received").Count;
                data.NumEmailsSentNow = Queries.GetEmailsSentOrReceivedFromSixAm(now, "sent").Count;
                data.NumChatsNow = Queries.GetChatsSentOrReceivedFromSixAm(now).Count;

                // get previous data
                var numMeetingsPrevious = new List<double>();
                var numEmailsReceivedPrevious = new List<double>();
                var numEmailsSentPrevious = new List<double>();
                var numChatsPrevious = new List<double>();
                var j = -6;
                for (var i = -1; i > j; i--)
                {
                    var previous = now.AddDays(i);
                    if (previous.DayOfWeek == DayOfWeek.Saturday || previous.DayOfWeek == DayOfWeek.Sunday)
                    {
                        j--;
                        continue;
                    }
                    numMeetingsPrevious.Add(Queries.GetMeetingsFromSixAm(previous).Count);
                    numEmailsReceivedPrevious.Add(Queries.GetEmailsSentOrReceivedFromSixAm(previous, "received").Count);
                    numEmailsSentPrevious.Add(Queries.GetEmailsSentOrReceivedFromSixAm(previous, "sent").Count);
                    numChatsPrevious.Add(Queries.GetChatsSentOrReceivedFromSixAm(previous).Count);
                }


                // calculate averages
                data.AvgMeetingsPrevious = Math.Ceiling(numMeetingsPrevious.Average());
                data.AvgEmailsReceivedPrevious = Math.Ceiling(numEmailsReceivedPrevious.Average());
                data.AvgEmailsSentPrevious = Math.Ceiling(numEmailsSentPrevious.Average());
                data.AvgChatsPrevious = Math.Ceiling(numChatsPrevious.Average());


                // calculate standard deviations
                data.MeetingsSD = Math.Ceiling(CalculateStdDev(numMeetingsPrevious));
                data.EmailsReceivedSD = Math.Ceiling(CalculateStdDev(numEmailsReceivedPrevious));
                data.EmailsSentSD = Math.Ceiling(CalculateStdDev(numEmailsSentPrevious));
                data.ChatsSD = Math.Ceiling(CalculateStdDev(numChatsPrevious));

                // for caching
                _previousInteractionDataSet = data;
                _lastTimeCalculatedDataSet = DateTime.Now;

                return data;
            }
            catch
            {
                return null;
            }
        }

        private static double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

    }
}
