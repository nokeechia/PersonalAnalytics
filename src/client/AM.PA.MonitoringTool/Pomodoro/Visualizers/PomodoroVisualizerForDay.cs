using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Helpers;

namespace Pomodoro.Visualizers
{
    public class PomodoroVisualizerForDay : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public PomodoroVisualizerForDay(DateTimeOffset date)
        {
            Title = "Completed Pomodoros";
            this._date = date;
            IsEnabled = true;
            Size = VisSize.Square;
            Order = 0;
        }

        public override string GetHtml()
        {
            List<Pomodoro> pomodorosOfDay = DatabaseConnector.GetPomodorosOfDay(_date);
            int totalPomodors = pomodorosOfDay.Count;
            double totalHours = pomodorosOfDay.Sum(x => x.Duration) / 60.0;

            var css =
                "<style>" +
                    ".pomodoroDayWrapper { " +
                        "text-align: center; padding: 10px;" +
                    "} " +
                    ".numbersWrapper { " +
                        "padding: 20px; font-size: 20px; font-weight: bold; " +
                    "} " +
                "</style>";

            var html = string.Empty;
            html += "<div class='pomodoroDayWrapper' id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='align: center; font-size: 1.15em;'>";
            html += "<div class='numbersWrapper'>";
            html += totalPomodors + " completed Pomodoros";
            html += "</div>";
            html += "<div class='numbersWrapper'>";
            html += totalHours.ToString("F2") + " hours of focused work";
            html += "</div>";
            html += css;
            html += "</div>";

            return html;
        }
    }
}
