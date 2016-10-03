// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-27
// 
// Licensed under the MIT License.

using System;
using Shared;
using Shared.Helpers;
using InteractionTracker.Data;
using System.Globalization;
using System.Collections.Generic;

namespace InteractionTracker.Visualizations
{
    internal class DayInteractionStepChart : BaseVisualization, IVisualization
    {
        public DayInteractionStepChart()
        {
            Title = "Today's Communication Timeline";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Wide2;
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
            
            int counter = -1;
            List<int> includes = new List<int>();
            foreach (var activity in chartQueryResultsLocal)
            {
                counter = -1;
                int previous = -1;

                foreach (var val in activity.Value)
                {
                    counter++;
                    if (previous != val || counter % 30 == 0)
                    {
                        previous = val;
                        includes.Add(counter-1);
                        includes.Add(counter);
                        includes.Add(counter+1);
                    }
                }
            }
            counter = -1;
            foreach (var activity in chartQueryResultsLocal)
            {
                counter = -1;
                var values = string.Empty;

                foreach (var val in activity.Value)
                {
                    counter++;
                    if (includes.Contains(counter))
                    {
                        values += val.ToString() + ", ";
                    }
                }

                columns += string.Format(CultureInfo.InvariantCulture, "['{0}', {1}], ", activity.Key, values);
            }
            counter = -1;
            while (earlier <= later)
            {
                counter++;

                if (earlier.Minute > 9)
                    hourMinute = String.Format(CultureInfo.InvariantCulture, "'{0}:{1}', ", earlier.Hour, earlier.Minute);
                else
                    hourMinute = String.Format(CultureInfo.InvariantCulture, "'{0}:0{1}', ", earlier.Hour, earlier.Minute);

                if (includes.Contains(counter) || earlier.Minute % 30 == 0)
                {
                    xList += hourMinute;

                    if (earlier.Minute % 30 == 0)
                    {
                        smallXList += hourMinute;
                    }
                }

                earlier = earlier.AddMinutes(1);
            }

            var nowMark = string.Empty;
            if (DateTime.Now.Minute > 9)
                nowMark = String.Format(CultureInfo.InvariantCulture, "'{0}:{1}'", DateTime.Now.Hour, DateTime.Now.Minute);
            else
                nowMark = String.Format(CultureInfo.InvariantCulture, "'{0}:0{1}'", DateTime.Now.Hour, DateTime.Now.Minute);

            // html += "<p style='text-align: center;'>Today's Communications</p>";
            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='height:75%;' align='center'></div>"
                    + "<script type='text/javascript'>"
                    + "var " + VisHelper.CreateChartHtmlTitle(Title) + " = c3.generate({ bindto: '#" + VisHelper.CreateChartHtmlTitle(Title) + "',data: { x:'x', xFormat:'%H:%M', columns:[['x', " + xList + "]," + columns + "], type:'area-step'}, selection: {enabled: true}, axis:{x:{show:true, tick:{rotate: 40, values: [" + smallXList + "], multiline:false, centered:false, fit:false}, type:'timeseries'}, y:{show:false}}, tooltip:{show:false}, padding: {left: 20, right: 20}, grid: {y:{lines: [{value: 1 }]}, x:{lines: [{value: " + nowMark + " }]}}, legend:{position:'inset', inset:{anchor:'top-right',x:10,y:10,step:undefined}}, transition:{duration:0}, interaction:{enabled:false}, point:{show:false}});" + VisHelper.CreateChartHtmlTitle(Title) + ".toggle(['Overall Communication']);"
                    + "</script>";

            return html;
        }
    }
}
