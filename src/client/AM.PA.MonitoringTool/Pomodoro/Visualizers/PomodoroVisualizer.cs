using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Helpers;
using System.Reflection;

namespace Pomodoro.Visualizers
{
    public class PomodoroVisualizer : BaseVisualizer
    {
        private bool _isEnabled = true;

        public PomodoroVisualizer()
        {
            Name = "Pomodoro Visualizer";

            //TODO: maybe move somewhere else?
            DatabaseConnector.CreatePomodorosTableIfNotExists();
        }

        public override bool IsEnabled()
        {
            return _isEnabled;
        }

        public override string GetVersion()
        {
            var v = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version;
            return VersionHelper.GetFormattedVersion(v);
        }

        public override List<IVisualization> GetVisualizationsDay(DateTimeOffset date)
        {
            var timerVis = new PomodoroTimerVisualizer(date);
            var dailyPomodorosVis = new PomodoroVisualizerForDay(date);
            return new List<IVisualization> { timerVis, dailyPomodorosVis };
        }
    }
}
