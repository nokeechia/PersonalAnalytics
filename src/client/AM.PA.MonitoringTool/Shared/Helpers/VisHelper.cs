// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-10-20
// 
// Licensed under the MIT License.

using Shared.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shared.Helpers
{
    public static class VisHelper
    {
        /// <summary>
        /// Returns a message that says that there is not enough data to
        /// provide a visualization and a standard message if no other is specified.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string NotEnoughData(string message = "We don't yet have enough data to show you a retrospection of your workday.")
        {
            return "<br/><div style='text-align: center; font-size: 0.66em;'>" + message + "</div>";
        }

        /// <summary>
        /// Returns a formated chart title with a specified title
        /// or a default one if there is none.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string FormatChartTitle(string title = "Visualization")
        {
            return "<h3 style='text-align: center;'>" + title + "</h3>";
        }

        /// <summary>
        /// just display an error message in red
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Error(string message = "An error occurred.")
        {
            return "<p style='color:red;'>" + message + "</p>";
        }

        /// <summary>
        /// Get a the width of the item
        /// (already reduced with default side margin)
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        //public static double GetChartWidthInPx(VisSize size, double addEm = 0)
        //{
        //    switch (size)
        //    {
        //        case VisSize.Small:
        //            return _genericBrowserDefaultSetting * (Settings.ItemSmallWidthEm + addEm);
        //        case VisSize.Square:
        //            return _genericBrowserDefaultSetting * (Settings.ItemSquareWidthEm + addEm);
        //        case VisSize.Wide:
        //            return _genericBrowserDefaultSetting * (Settings.ItemWideWidthEm + addEm);
        //        default:
        //            return _genericBrowserDefaultSetting * (Settings.ItemSquareWidthEm + addEm);
        //    }
        //}

        /// <summary>
        /// Get a the height of the item
        /// (already reduced with default title margin)
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        //public static double GetChartHeightInPx(VisSize size, double addEm = 0)
        //{
        //    switch (size)
        //    {
        //        case VisSize.Small:
        //            return _genericBrowserDefaultSetting * (Settings.ItemSmallHeightEm + addEm);
        //        case VisSize.Square:
        //            return _genericBrowserDefaultSetting * (Settings.ItemSquareHeightEm + addEm);
        //        case VisSize.Wide:
        //            return _genericBrowserDefaultSetting * (Settings.ItemWideHeightEm + addEm);
        //        default:
        //            return _genericBrowserDefaultSetting * (Settings.ItemSquareHeightEm + addEm);
        //    }
        //}

        //private static int _genericBrowserDefaultSetting = 16; //px

        /// <summary>
        /// Gets the visualization name and creates a simple chart title
        /// by removing the whitespaces and lowercasing
        /// </summary>
        /// <param name="VisName"></param>
        /// <returns></returns>
        public static string CreateChartHtmlTitle(string visName)
        {
            var removeStuffInBrackets1 = @"(\[([^\]]*)\])+";
            var removeStuffInBrackets2 = @"(\(([^\)]*)\))+";
            var removeEverythingButLetters = "[^a-zA-Z]";

            var removed = Regex.Replace(visName, removeStuffInBrackets1, "");
            removed = Regex.Replace(removed, removeStuffInBrackets2, "");
            removed = Regex.Replace(removed, removeEverythingButLetters, "");

            return removed.ToLower();
        }

        /// <summary>
        /// Prepares the timeline axis,
        /// setting the first and last entry of the workday as start and end point
        /// </summary>
        /// <param name="date"></param>
        /// <param name="dto"></param>
        /// <param name="interval"></param>
        public static void PrepareTimeAxis(DateTimeOffset date, Dictionary<DateTime, int> dto, int interval)
        {
            var min = Database.GetInstance().GetUserWorkStart(date);
            min = min.AddSeconds(-min.Second); // nice seconds
            min = DateTimeHelper.RoundUp(min, TimeSpan.FromMinutes(-interval)); // nice minutes

            var max = Database.GetInstance().GetUserWorkEnd(date); //GetUserLastMiniSurveyEntry(date);
            max = max.AddSeconds(-max.Second); // nice seconds
            max = DateTimeHelper.RoundUp(max, TimeSpan.FromMinutes(interval)); // nice minutes

            while (min < max)
            {
                var key = min.AddMinutes(interval);
                dto.Add(key, 0);
                min = key;
            }
        }

        public class DateElement<T>
        {
            public String date;
            public T count;
        }

        public class DateElementExtended<T>
        {
            public String date;
            public T normalizedvalue;
            public T originalvalue;
            public String extraInfo;
        }

        public class ExtraInfo
        {
            public int switches { get; set; }
            public string topPgms { get; set; }
        }
        public static String CreateJavaScriptArrayOfObjects(List<Tuple<DateTime, int >> t)
        {
            List<DateElement<int>> dateElements = new List<DateElement<int>>();
              foreach (Tuple<DateTime, int> i in t)
            {
                DateTime jsDate = i.Item1;
                dateElements.Add(new DateElement<int>() { date = jsDate.ToString(), count = i.Item2 });

            }

            var output = dateElements.Aggregate("", (current, b) => current + "{date: '" + b.date.ToString() + "', count: " + b.count + "},").Trim().TrimEnd(',');

            return output;
        }

        //todo clean up code no two methods
        public static String CreateJavaScriptArrayOfObjectsDouble(List<Tuple<DateTime, double>> t)
        {
            List<DateElement<double>> dateElements = new List<DateElement<double>>();
            foreach (Tuple<DateTime, double> i in t)
            {
                DateTime jsDate = i.Item1;
                dateElements.Add(new DateElement<double>() { date = jsDate.ToString(), count = Math.Round(i.Item2,2) });

            }

            var output = dateElements.Aggregate("", (current, b) => current + "{date: '" + b.date.ToString() + "', count: " + b.count + "},").Trim().TrimEnd(',');

            return output;
        }

        public static String CreateJavaScriptArrayOfObjectsDoubleWithAddtionalInfo(List<DateElementExtended<double>> d)
        {
            return d.Aggregate("", (current, b) => current + "{date: '" + b.date.ToString() + "', normalizedvalue: '" + b.normalizedvalue + "', originalvalue: '" + b.originalvalue + "', extraInfo: '" + b.extraInfo + "'},").Trim().TrimEnd(','); 
        }

        public static double Normalize(double x, double min, double max)
        {
            //f(x) = (x-min) / (max-min)            
            return Math.Round((double)(x - min) / (max - min), 2);
        }

        public static String calculateIdleRegions(List<Tuple<DateTime, int>> items)
        {
            var idleRegionsAsStr = "";
            var index = 0;
            var maxIndex = items.Count() - 1;
            List<Tuple<DateTime, DateTime>> allRegions = new List<Tuple<DateTime, DateTime>>();
            List<Tuple<long, long>> idleRegions = new List<Tuple<long, long>>();

            foreach (var i in items)
            {
                if (index + 1 <= maxIndex)
                {
                    allRegions.Add(Tuple.Create(i.Item1, items.ElementAt(index + 1).Item1));
                }
                index++;
            }

            foreach (var i in allRegions)
            {
                TimeSpan timeSpan = i.Item2.Subtract(i.Item1);

                if (timeSpan.TotalMinutes > 30)
                {
                    idleRegions.Add(Tuple.Create(DateTimeHelper.JavascriptTimestampFromDateTime(Convert.ToDateTime(i.Item1)), DateTimeHelper.JavascriptTimestampFromDateTime(Convert.ToDateTime(i.Item2))));
                }
            }

            idleRegionsAsStr = idleRegions.Aggregate("", (current, a) => current + ( "{ 'start':" + a.Item1 + ",'end':" + a.Item2 + ",'style':'dashed'}, ")).TrimEnd(',');

            return idleRegionsAsStr;

        }
    }
}
