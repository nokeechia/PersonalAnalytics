// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-27
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Helpers;
using InteractionTracker.Data;
using System.Globalization;
using System.Runtime.InteropServices;

namespace InteractionTracker.Visualizations
{
    internal class DayInteractionStepChart : BaseVisualization, IVisualization
    {
        public DayInteractionStepChart()
        {
            Title = "Today's Interaction Breakdown";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetHtml()
        {
            var html = string.Empty;
            var chartQueryResultsLocal = Queries.GetActivityStepChartData();

            if (chartQueryResultsLocal.Count == 0)
            {
                html += VisHelper.NotEnoughData(Dict.NotEnoughData);
                return html;
            }

            var hourMinute = string.Empty;
            var xList = string.Empty;
            var smallXList = string.Empty;
            var now = DateTime.Now;
            var earlier = now.Date.AddHours(6);
            var columns = string.Empty;

            for (; earlier <= now;)
            {
                if (earlier.Minute > 9)
                    hourMinute = String.Format(CultureInfo.InvariantCulture, "'{0}:{1}', ", earlier.Hour, earlier.Minute);
                else
                    hourMinute = String.Format(CultureInfo.InvariantCulture, "'{0}:0{1}', ", earlier.Hour, earlier.Minute);

                xList += hourMinute;

                if (earlier.Minute % 15 == 0)
                {
                    smallXList += hourMinute;
                }

                earlier = earlier.AddMinutes(1);
            }

            foreach (var activity in chartQueryResultsLocal)
            {
                var values = string.Empty;

                foreach (var val in activity.Value)
                {
                    values += val.ToString() + ", ";
                }

                columns += string.Format(CultureInfo.InvariantCulture, "['{0}', {1}], ", activity.Key, values);
            }

            // html += "<p style='text-align: center;'>Today's Interactions</p>";
            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='height:75%;'  align='center'></div>";

            html += "<script type='text/javascript'>";
            html +=
                "var " + VisHelper.CreateChartHtmlTitle(Title) + " = c3.generate({ bindto: '#" + VisHelper.CreateChartHtmlTitle(Title) + "',data: { x:'x', xFormat:'%H:%M', columns:[['x', " + xList + "]," + columns + "], type:'step'},axis:{x:{show:true, tick:{rotate: 40, values: [" + smallXList + "], multiline:false, centered:true, fit:true}, type:'timeseries'}, y:{show:false}}, tooltip:{show:false}, padding: {left: 20, right: 20}});";
            html += "</script>";

            return html;
        }
    }
}
/*
 *  "var " + VisHelper.CreateChartHtmlTitle(Title) + " = c3.generate({ bindto: '#" + VisHelper.CreateChartHtmlTitle(Title) + "',data: { x:'x', columns:[['x', " + xList + ",['data1', 1, 1, 1, 0, 0, 1],['data2',1,1,1,0,0,0],['data3',1,0,1,0,0,1],['data4',0,0,0,0,0,0]," + columns + "],names: {data1: 'Chats Received', data2: 'Chats Sent', data3: 'Emails Received', data4: 'Emails Sent', data5: 'Meetings'},types:{data1:'step',data2:'step',data3:'step',data4:'step',data5:'step'}},axis:{x:{show:true, tick:{count: 24, culling: {max: 25}},type:'category'},y:{show:false}},tooltip:{show:false}});";
 */

//count: 24,