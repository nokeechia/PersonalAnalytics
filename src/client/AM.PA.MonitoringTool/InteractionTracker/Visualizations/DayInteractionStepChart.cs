// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-27
// 
// Licensed under the MIT License.

using System;
using Shared;
using Shared.Helpers;
using InteractionTracker.Data;
using System.Globalization;

namespace InteractionTracker.Visualizations
{
    internal class DayInteractionStepChart : BaseVisualization, IVisualization
    {
        public DayInteractionStepChart()
        {
            Title = "Interaction Details";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            var now = DateTime.Now;
            var earlier = now.Date.AddHours(6);
            var later = now.Date.AddHours(18);
            var chartQueryResultsLocal = Queries.GetActivityStepChartData(earlier, later);

            if (chartQueryResultsLocal.Count == 0)
            {
                html += VisHelper.NotEnoughData(Dict.NotEnoughData);
                return html;
            }

            var hourMinute = string.Empty;
            var xList = string.Empty;
            var smallXList = string.Empty;
            var columns = string.Empty;

            for (; earlier <= later;)
            {
                if (earlier.Minute > 9)
                    hourMinute = String.Format(CultureInfo.InvariantCulture, "'{0}:{1}', ", earlier.Hour, earlier.Minute);
                else
                    hourMinute = String.Format(CultureInfo.InvariantCulture, "'{0}:0{1}', ", earlier.Hour, earlier.Minute);

                xList += hourMinute;

                if (earlier.Minute % 30 == 0)
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
                "var " + VisHelper.CreateChartHtmlTitle(Title) + " = c3.generate({ bindto: '#" + VisHelper.CreateChartHtmlTitle(Title) + "',data: { x:'x', xFormat:'%H:%M', columns:[['x', " + xList + "]," + columns + "], type:'area-step'}, selection: {enabled: true}, axis:{x:{show:true, tick:{rotate: 40, values: [" + smallXList + "], multiline:false, centered:true, fit:true}, type:'timeseries'}, y:{show:false}}, tooltip:{show:false}, padding: {left: 20, right: 20}, grid: {y:{lines: [{value: 1 }]}}});" + VisHelper.CreateChartHtmlTitle(Title) + ".toggle(['Emails Received']);" + VisHelper.CreateChartHtmlTitle(Title) + ".toggle(['Overall Interactions']);";
            html += "</script>";

            return html;
        }
    }
}
