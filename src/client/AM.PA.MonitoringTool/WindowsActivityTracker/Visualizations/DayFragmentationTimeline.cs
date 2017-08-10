﻿// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2017-01-03
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Helpers;
using WindowsActivityTracker.Data;
using WindowsActivityTracker.Models;
using Shared.Data.Extractors;

namespace WindowsActivityTracker.Visualizations
{
    internal class DayFragmentationTimeline : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;
        private const int _timelineZoomFactor = 1; // shouldn't be 0!, if > 1 then the user can scroll on top of the timeline

        public DayFragmentationTimeline(DateTimeOffset date)
        {
            this._date = date;

            Title = "Timeline: Activities over the Day"; //hint; overwritten below
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var orderedTimelineList = Queries.GetDayTimelineData(_date);

            /////////////////////
            // data cleaning
            /////////////////////

            // show message if not enough data
            if (orderedTimelineList.Count <= 3) // 3 is the minimum number of input-data-items
            {
                html += VisHelper.NotEnoughData(Dict.NotEnoughData);
                return html;
            }

            // remove first + last items if IDLE
            if (orderedTimelineList.First().ActivityCategory == ActivityCategory.Idle)
            {
                orderedTimelineList.Remove(orderedTimelineList.First());
            }
            if (orderedTimelineList.Last().ActivityCategory == ActivityCategory.Idle)
            {
                orderedTimelineList.Remove(orderedTimelineList.Last());
            }

            /////////////////////
            // Create HTML
            /////////////////////

            html += GetActivityVisualizationContent(orderedTimelineList);

            return html;
        }

        private string GetActivityVisualizationContent(List<WindowsActivity> activityList)
        {
            var categories = activityList.Select(a => a.ActivityCategory).Distinct().OrderBy(a => a).ToList();
            string activityTimeline = "activityTimeline";
            const string defaultHoverText = "Hint: Hover over the timeline to see details.";

            var html = string.Empty;

            /////////////////////
            // CSS
            /////////////////////

            html += @"<style type='text/css'>
                    .axis path,
                    .axis line {
                      fill: none;
                      stroke: black;
                      shape-rendering: crispEdges;
                    }
                    .axis text {
                      font-size: .71em;
                    }
                    .timeline-label {
                      font-size: .71em;
                    }
                    </style>";

            /////////////////////
            // Javascript
            /////////////////////

            html += @"<script type='text/javascript'>
                    var onLoad = window.onload;
                    window.onload = function() { 
                    if (typeof onLoad == 'function') { onLoad(); } ";

            // create formatted javascript data list
            html += "var data = [" + CreateJavascriptActivityDataList(activityList) + "]; ";

            // create color scale
            html += CreateColorScheme(categories);

            // width & height
            html += "var itemWidth = 0.98 * document.getElementsByClassName('item Wide')[0].offsetWidth;";
            html += "var itemHeight = 0.13 * document.getElementsByClassName('item Wide')[0].offsetHeight;";

            // hover Event (d: current rendering object, i: index during d3 rendering, data: data object)
            var hover = @".hover(function(d, i, data) { 
                            console.log(d);
                            console.log(data);

                            document.getElementById('hoverDetails').innerHTML = '<span style=\'font-size:1.2em; color:#007acc;\'>From ' + d['starting_time_formatted'] + ' to ' + d['ending_time_formatted'] + ' (' + d['duration'] + 'min)</span>' +
                                                                                '<br /><strong>Activity</strong>: <span style=\'color:' + d['color'] + '\'>■</span> ' + d['activity'] +
                                                                                '<br /><strong>Processes</strong>: ' + d['processes'] + 
                                                                                '<br /><strong>Window Titles</strong>: ' + d['window_titles']; 
                        })";

            // mouseout Event
            var mouseout = @".mouseout(function (d, i, datum) { document.getElementById('hoverDetails').innerHTML = '" + defaultHoverText + "'; })";

            // define configuration
            html += "var " + activityTimeline + " = d3.timeline().width(" + _timelineZoomFactor + " * itemWidth).itemHeight(itemHeight)" + hover + mouseout + ";"; // .colors(colorScale).colorProperty('activity') // .stack()
            html += "var svg = d3.select('#" + activityTimeline + "').append('svg').attr('width', itemWidth).datum(data).call(" + activityTimeline + "); ";

            html += "}; "; // end #1
            html += "</script>";

            /////////////////////
            // HTML
            /////////////////////

            // show details on hover
            html += "<div style='height:37%; style='align: center'><p id='hoverDetails'>"+ defaultHoverText + "</p></div>";

            // add timeline
            html += "<div id='" + activityTimeline + "' align='center'></div>";

            // add legend 
            html += GetLegendForCategories(categories);

            return html;
        }

        /// <summary>
        /// prepares a formatted javascript list of the participantActivityData
        /// </summary>
        /// <param name="participantActivityData"></param>
        /// <returns></returns>
        private string CreateJavascriptActivityDataList(List<WindowsActivity> activityList)
        {
            var html = string.Empty;

            var categories = activityList.Select(a => a.ActivityCategory).Distinct().ToList();

            foreach (var category in categories)
            {
                var times = string.Empty;
                foreach (var activityEntry in activityList.Where(a => a.ActivityCategory == category))
                {
                    var startTime = JavascriptTimestampFromDateTime(activityEntry.StartTime);
                    var endTime = JavascriptTimestampFromDateTime(activityEntry.EndTime);

                    // add data used for the timeline and the timeline hover
                    times += "{'starting_time': " + startTime + ", 'ending_time': " + endTime + 
                             ", 'starting_time_formatted': '" + activityEntry.StartTime.ToShortTimeString() + 
                             "', 'ending_time_formatted': '" + activityEntry.EndTime.ToShortTimeString() + 
                             "', 'duration': " + Math.Round(activityEntry.DurationInSeconds / 60.0, 1) + 
                             ", 'window_titles': '" + ReadableWindowTitles(activityEntry.WindowProcessList) +
                             "', 'processes': '" + ReadableProcesses(activityEntry.WindowProcessList) + 
                             "', 'color': '" + GetHtmlColorForContextCategory(activityEntry.ActivityCategory) +
                             "', 'activity': '" + GetDescriptionForContextCategory(activityEntry.ActivityCategory) + "'}, ";
                }

                html += "{activity: '" + category + "', times: [" + times + "]}, ";
            }

            return html;
        }

        #region Readable WindowTitle and Process

        private string ReadableWindowTitles(List<WindowProcessItem> list)
        {
            var maxNumItems = 5;
            var str = string.Empty;

            // distinct items
            var windowTitles = list.Select(i => i.WindowTitle).Distinct().ToList();

            // only maxNumItems
            if (windowTitles.Count > maxNumItems)
            {
                for (int i = 0; i < maxNumItems; i++)
                {
                    str += FormatWindowTitle(windowTitles[i]);
                }
                str += " and " + (windowTitles.Count - maxNumItems) + " more.";
            }
            else
            {
                foreach (var item in windowTitles) str += FormatWindowTitle(item);
            }
            return str.Trim().TrimEnd(',').Replace("/", "//").Replace(@"\", @"\\");
        }

        private string FormatWindowTitle(string windowTitle)
        {
            return string.IsNullOrEmpty(windowTitle) ? string.Empty : windowTitle.Replace("'", "") + ", ";
        }

        private string ReadableProcesses(List<WindowProcessItem> list)
        {
            // distinct processes
            var processes = list.Select(i => i.Process).Distinct().ToList();

            // build readable string
            var str = string.Empty;
            foreach (var item in processes) str += FormatProcesses(item);
            return str.Trim().TrimEnd(',');
        }

        private string FormatProcesses(string process)
        {
            return string.IsNullOrEmpty(process) ? string.Empty : ProcessNameHelper.GetFileDescription(process).Replace("'", "") + ", ";
        }

        #endregion

        #region Helpers for legend and colors

        /// <summary>
        /// Creates a colored square for each category (legend)
        /// </summary>
        /// <param name="activityDataSet"></param>
        /// <param name="visWidth"></param>
        /// <returns></returns>
        private string GetLegendForCategories(List<ActivityCategory> categoryList)
        {
            var html = string.Empty;
            html += @"<style type='text/css'>
                    #legend li { display: inline-block; padding-right: 1em; list-style-type: square; }
                    li:before { content: '■ '} 
                    li span { font-size: .71em; color: black;} 
                    </style>";

            html += "<div><ul id='legend' align='center'>" // style='width:" + visWidth + "px'
                   +  categoryList.Where(c => c != ActivityCategory.Idle).Aggregate(string.Empty, (current, cat) => current + ("<li style='color:" + GetHtmlColorForContextCategory(cat) + "'><span>" + GetDescriptionForContextCategory(cat) + "</span></li>"))
                   +  "</ul></div>";

            return html;
        }

        private string GetLegendEntryForActivity(ActivityCategory category)
        {
            return "<li style='color:" + GetHtmlColorForContextCategory(category) + "'><span>" + GetDescriptionForContextCategory(category) + "</span></li>";
        }

        /// <summary>
        /// Creates a colorscheme for each activity category
        /// </summary>
        /// <param name="activityList"></param>
        /// <returns></returns>
        private string CreateColorScheme(List<ActivityCategory> categories)
        {
            var rangeString = categories.Aggregate(string.Empty, (current, item) => current + ("'" + GetHtmlColorForContextCategory(item) + "', "));
            var activityString = categories.Aggregate(string.Empty, (current, item) => current + ("'" + item + "', "));

            var html = "var colorScale = d3.scale.ordinal().range([" + rangeString + "]).domain([" + activityString + "]); ";

            return html;
        }

        /// <summary>
        /// Return a color for each context category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private string GetHtmlColorForContextCategory(ActivityCategory category)
        {
            var noneColor = "#DDDDDD";

            switch (category)
            {
                case ActivityCategory.DevCode:
                    return "#A547D1";//"#6B238E";//"#00b300"; //"#007acb"; // darker blue
                case ActivityCategory.DevDebug:
                    return "#C91BC7";//"#8EC4E8"; // fairest blue
                case ActivityCategory.DevReview:
                    return "#D7ADEB"; //"#1484CE"; //fairest blue
                case ActivityCategory.DevVc:
                    return "#F9D1F8";// "#1484CE"; //fairest blue   !!!
                case ActivityCategory.ReadWriteDocument:
                //case ActivityCategory.ManualEditor:
                //return "#00cc00";//"#36c1c4"; // another blue
                case ActivityCategory.PlannedMeeting:
                    return "#99EBFF";//#87CEEB";// "#3258E6";//  "#00b300"; // dark green
                case ActivityCategory.InformalMeeting:
                case ActivityCategory.InstantMessaging:
                    return "#12A5F4";// "#C91BC7";//"#00cc00"; // fair green
                case ActivityCategory.Planning:
                    return "#9DB7E8"; // "#F9D1F8";//"#e855e8"; // dark violett
                case ActivityCategory.Email:
                    return "#326CCC";// "#2858a5";//"#ED77EB"; // fair violett
                //case ActivityCategory.WebBrowsing:
                case ActivityCategory.WorkRelatedBrowsing:
                    return "#FF9333"; //orange "#FFE3CB";//"#FFA500"; 
                case ActivityCategory.WorkUnrelatedBrowsing:
                    return "#FFC999"; // "red"; // fair orange
                case ActivityCategory.FileNavigationInExplorer:
                    return "#d3d3d3"; // light gray
                case ActivityCategory.Other:
                case ActivityCategory.OtherRdp:
                //case ActivityCategory.OtherMusic:
                case ActivityCategory.Unknown:
                    return "gray";
                case ActivityCategory.Idle:
                    return "white";
                case ActivityCategory.Uncategorized:
                    return noneColor; 
            }

            return noneColor; // default color
        }

        /// <summary>
        /// Return a screen name for the activity category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private string GetDescriptionForContextCategory(ActivityCategory category)
        {
            switch (category)
            {
                case ActivityCategory.DevCode:
                    return "Development";
                case ActivityCategory.DevDebug:
                    return "Debugger Use";
                case ActivityCategory.DevVc:
                    return "Version Control";
                case ActivityCategory.DevReview:
                    return "Code Reviewing";
                case ActivityCategory.ReadWriteDocument:
                    return "Reading/Editing Documents";
                case ActivityCategory.InformalMeeting:
                case ActivityCategory.InstantMessaging:
                    return "Instant Messaging"; // Ad-Hoc Meeting
                case ActivityCategory.PlannedMeeting:
                    return "Scheduled meetings";
                case ActivityCategory.Planning:
                    return "Planning";
                case ActivityCategory.Email:
                    return "Emails";
                //case ActivityCategory.WebBrowsing:
                //    return "Browsing (uncategorized)";
                case ActivityCategory.WorkRelatedBrowsing:
                    return "Browsing work-related";// "Work related browsing";
                case ActivityCategory.WorkUnrelatedBrowsing:
                    return "Browsing work-unrelated";// "Work un-related browsing";
                case ActivityCategory.FileNavigationInExplorer:
                    return "Navigation in File Explorer";
                case ActivityCategory.Other:
                    return "Other";
                //case ActivityCategory.OtherMusic:
                //    return "OtherMusic";
                case ActivityCategory.Unknown:
                    return "Uncategorized";
                case ActivityCategory.OtherRdp:
                    return "RDP (uncategorized)";
                case ActivityCategory.Idle:
                    return "Idle (e.g. break, lunch, meeting)";
            }

            return "??"; // default color
        }

        #endregion

        #region Other Helpers

        private static long JavascriptTimestampFromDateTime(DateTime date)
        {
            var datetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
            return ((date.ToUniversalTime().Ticks - datetimeMinTimeTicks) / 10000);
            // return (date.Ticks - 621355968000000000)/10000; //old: had wrong timezone
        }

        #endregion
    }
}
