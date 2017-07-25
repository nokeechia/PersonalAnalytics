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
    public enum PomodoroState { Pomodoro, Pause, ShortBreak, LongBreak, Idle }

    public class PomodoroTimerVisualizer : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;
        private Pomodoro _currentPomodoro;
        private int _currentIteration;
        private PomodoroState _state; 

        public PomodoroTimerVisualizer(DateTimeOffset date)
        {
            Title = "Pomodoro Timer";
            this._date = date;
            IsEnabled = date.Date == DateTime.Now.Date; // pomodoros can only be timed for the current day
            Size = VisSize.Square;
            Order = 0;

            _currentIteration = 0;
            _state = PomodoroState.Idle;

            //subscribe for Pomodoro events if not yet attached
            if (!ObjectForScriptingHelper.PomodoroStartedEventHandlerAttached("PomodoroTimerStarted"))
            {
                ObjectForScriptingHelper.PomodoroTimerStarted += PomodoroTimerStarted;
            }
            if (!ObjectForScriptingHelper.PomodoroPausedEventHandlerAttached("PomodoroTimerPaused"))
            {
                ObjectForScriptingHelper.PomodoroTimerPaused += PomodoroTimerPaused;
            }
            if (!ObjectForScriptingHelper.PomodoroStoppedEventHandlerAttached("PomodoroTimerStopped"))
            {
                ObjectForScriptingHelper.PomodoroTimerStopped += PomodoroTimerStopped;
            }
            if (!ObjectForScriptingHelper.PomodoroCompletedEventHandlerAttached("PomodoroTimerCompleted"))
            {
                ObjectForScriptingHelper.PomodoroTimerCompleted += PomodoroTimerCompleted;
            }
        }

        public override string GetHtml()
        {
            var script =
                "<script>" +
                    "var timer = new Timer(); " +
                    "$('#countdown').html(timer.getTimeValues().toString()); " +
                    "timer.addEventListener('secondsUpdated', function(e) { " +
                        "$('#countdown').html(timer.getTimeValues().toString(['minutes', 'seconds'])); " +
                        "if (timer.getTimeValues().minutes <= 5 && !$('#pomodoroTimerWrapper').hasClass('timeCueLast5min')) {" +
                            "$('#pomodoroTimerWrapper').addClass('timeCueLast5min');" +
                            "$('#pomodoroTimerWrapper').removeClass('timeCueNormal'); " +
                        "} " +
                        "if (timer.getTimeValues().minutes <= 0 && timer.getTimeValues().seconds <= 15) {" +
                            "$('#pomodoroTimerWrapper').fadeToggle('slow', 'linear');" +
                        "} " +
                    "}); " +
                    "timer.addEventListener('targetAchieved', function(e) { " +
                        "$('#text').html('You made it! <br\\>Time for a break. =)'); " +
                        "window.external.JS_PomodoroTimerCompleted(); " +
                        "$('#startButton').show(); " +
                        "$('#pauseButton').hide(); " +
                        "$('#stopButton').hide(); " +
                        "$('#pomodoroTimerWrapper').removeClass('timeCueLast5min');" +
                    "}); " +
                    "$('#startButton').click(function() { " +
                        "timer.start({countdown: true, startValues: {seconds: " + 60 * Settings.DefaultPomodoroDuration + "}}); " +
                        "$('#text').html('Yes! Let&#39;s work!'); " +
                        "$('#startButton').hide(); " +
                        "$('#pauseButton').show(); " +
                        "$('#stopButton').show(); " +
                        "if (timer.getTimeValues().minutes <= 5) { " +
                            "$('#pomodoroTimerWrapper').addClass('timeCueLast5min');" +
                        "} else { " +
                            "$('#pomodoroTimerWrapper').addClass('timeCueNormal');" +
                        "} " +
                    "}); " + 
                    "$('#pauseButton').click(function() { " +
                        "timer.pause(); " +
                        "$('#text').html('Come on, you can finish this!'); " +
                        "$('#startButton').show(); " +
                        "$('#pauseButton').hide(); " +
                        "$('#stopButton').show(); " +
                        "$('#pomodoroTimerWrapper').removeClass('timeCueNormal');" +
                        "$('#pomodoroTimerWrapper').removeClass('timeCueLast5min');" +
                    "}); " +
                    "$('#stopButton').click(function() { " +
                        "timer.stop(); " +
                        "$('#countdown').html('" + Settings.DefaultPomodoroDuration + ":00'); " +
                        "$('#text').html('Oh no, you stopped! <br\\>Let&#39;s start again!'); " +
                        "$('#startButton').show(); " +
                        "$('#pauseButton').hide(); " +
                        "$('#stopButton').hide(); " +
                        "$('#pomodoroTimerWrapper').removeClass('timeCueNormal');" +
                        "$('#pomodoroTimerWrapper').removeClass('timeCueLast5min');" +
                    "}); " +
                    "$('#startButton').show(); " +
                    "$('#pauseButton').hide(); " +
                    "$('#stopButton').hide(); " +
            "</script>";

            var css = 
                "<style>" +
                    "#pomodoroTimerWrapper { " +
                        "text-align: center; padding: 10px; border-radius:5px;" +
                    "} " +
                    "#countdown { " +
                        "padding: 20px; font-size: 20px; font-weight: bold; " +
                    "} " +
                    "#pomodoroTimerWrapper .button { " +
                        "border-radius: 14px; display: inline-block; cursor: pointer; color: rgb(255, 255, 255);  font-size: 18px; font-weight:bold; margin: 3px; padding: 13px 25px; text-decoration: none; " +
                    "} " +
                    "#startButton:hover { " +
                        "background-color: rgb(50, 181, 85); " +
                    "} " +
                    "#startButton { " +
                        "background-color: rgb(86, 204, 115); " +
                    "} " +
                    "#pauseButton:hover { " +
                        "background-color: rgb(70, 73, 227); " +
                    "} " +
                    "#pauseButton { " +
                        "background-color: rgb(107, 109, 237); " +
                    "} " +
                    "#stopButton:hover { " +
                        "background-color: rgb(209, 67, 67); " +
                    "} " +
                    "#stopButton { " +
                        "background-color: rgb(230, 110, 110); " +
                    "} " +
                    "#text { " +
                        "padding: 20px; font-size: 20px; " +
                    "} " +
                    ".timeCueNormal { " +
                        "background-color: #f3abab; " +
                    "} " +
                    ".timeCueLast5min { " +
                        "background-color: #e1bee7; " +
                    "} " +
                "</style>";

            var html = string.Empty;
            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='align: center; font-size: 1.15em;'>";
            html += "<div id='pomodoroTimerWrapper'> ";
            html += "<div id='countdown'>25:00</div>";
            html += "<div>";
            html += "<div id='startButton' class='button' onclick='window.external.JS_PomodoroTimerStarted()'>Start</div>";
            html += "<div id='pauseButton' class='button' onclick='window.external.JS_PomodoroTimerPaused()'>Pause</div>";
            html += "<div id='stopButton' class='button' onclick='window.external.JS_PomodoroTimerStopped()'>Stop</div>";
            html += "</div>";
            html += "<div id='text'/>";
            html += "Let&#39;s get started!";
            html += "</div>";
            html += "</div>";
            html += css;
            html += script;
            html += "</div>";

            return html;
        }

        private void PomodoroTimerStarted()
        {
            if (_state == PomodoroState.Idle)
            {
                _currentPomodoro = new Pomodoro() { StartTime = DateTime.Now, Duration = Settings.DefaultPomodoroDuration };
                _state = PomodoroState.Pomodoro;
            }
            else if (_state == PomodoroState.Pause)
            {
                _currentPomodoro.PausedResumed.Add(new Tuple<string, DateTime>("Resumed", DateTime.Now));
                _state = PomodoroState.Pomodoro;
            }
        }

        private void PomodoroTimerPaused()
        {
            if (_state == PomodoroState.Pomodoro)
            {
                _currentPomodoro.PausedResumed.Add(new Tuple<string, DateTime>("Paused", DateTime.Now));
                _state = PomodoroState.Pause;
            }
        }

        private void PomodoroTimerStopped()
        {
            if (_state == PomodoroState.Pomodoro || _state == PomodoroState.Pause)
            {
                _state = PomodoroState.Idle;
            }
        }

        private void PomodoroTimerCompleted()
        {
            if (_state == PomodoroState.Pomodoro)
            {
                _currentPomodoro.EndTime = DateTime.Now;
                DatabaseConnector.AddPomodoro(_currentPomodoro);
                _state = PomodoroState.Idle;
            }
        }
    }
}
