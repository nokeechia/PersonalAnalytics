using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Helpers;
using Retrospection;

namespace Pomodoro.Visualizers
{
    public class PomodoroTimerVisualizer : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;
        public PomodoroTimerVisualizer(DateTimeOffset date)
        {
            Title = "Pomodoro Timer";
            this._date = date;
            IsEnabled = date.Date == DateTime.Now.Date;
            Size = VisSize.Small;
            Order = 0;

            ObjectForScriptingHelper.PomodoroTimerClicked += PomodoroTimerClicked;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='align: center; font-size: 1.15em;'>";
            html += "<div class='countdown'></div>";
            html += "<button type='button' onclick =\"window.external.JS_PomodoroTimerClicked()\" >CLICK ME!</button> ";
            html += "</div>";

            return html;
        }

        private void PomodoroTimerClicked(string test)
        {
            Console.WriteLine(test);
        }
    }
}
