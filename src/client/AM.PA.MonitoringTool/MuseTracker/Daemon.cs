// Created by Monica Trink fom the University of Zurich
// Created: 2016-07-09
// 
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Shared;
using SharpOSC;
using System.IO;
using System.Timers;
using MuseTracker.Models;
using MuseTracker.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MuseTracker.Visualizations;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace MuseTracker
{
    public enum MuseContactStates
    {
        Good = 1,
        Bad = 0,
        Nocontact = -1
    }

    public class Daemon : BaseTrackerDisposable, ITracker
    {
        #region FIELDS
        private bool _disposed = false;
        private System.Timers.Timer _saveToDatabaseTimer;
        private UDPListener _listener;
        private System.Timers.Timer _checkMuseConnectionTimer;

        private int _pid = 0;

        // buffers for user input, they are emptied every 60s (Settings.IntervalSaveToDatabaseInSeconds)
        private static readonly ConcurrentQueue<MuseEEGDataEvent> MuseEEGDataBuffer = new ConcurrentQueue<MuseEEGDataEvent>();
        private static readonly ConcurrentQueue<MuseEEGDataQuality> MuseEEGDataQualityBuffer = new ConcurrentQueue<MuseEEGDataQuality>();
        private static readonly ConcurrentQueue<MuseBlinkEvent> MuseBlinkBuffer = new ConcurrentQueue<MuseBlinkEvent>();
        private static readonly ConcurrentQueue<MuseConcentrationEvent> MuseConcentrationBuffer = new ConcurrentQueue<MuseConcentrationEvent>();
        private static readonly ConcurrentQueue<MuseMellowEvent> MuseMellowBuffer = new ConcurrentQueue<MuseMellowEvent>();

        private static Func<double, int> ZeroOrOne = (x => x > 0.5 ? 1 : 0);

        public static MuseContactStates CurrentMuseContactState = MuseContactStates.Nocontact;
        public static DateTime museContactTsd = DateTime.Now;
        public static float RemainingBattery = -100;

        public static MuseChannelQuality QualityLeft = MuseChannelQuality.Nocontact;
        public static MuseChannelQuality QualityTopLeft = MuseChannelQuality.Nocontact;
        public static MuseChannelQuality QualityTopRight = MuseChannelQuality.Nocontact;
        public static MuseChannelQuality QualityRight = MuseChannelQuality.Nocontact;

        #endregion

        #region METHODS

        #region ITracker Stuff

        public Daemon()
        {
            Name = "Muse Tracker";
        }


        public enum MuseChannelQuality: int
        {
            Good = 1,
            OK = 2,
            Bad = 3,
            Nocontact = 4
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _saveToDatabaseTimer.Dispose();
                    _checkMuseConnectionTimer.Dispose();
                    _listener.Dispose();
                }

                // Release unmanaged resources.
                // Set large fields to null.                
                _disposed = true;
            }

            // Call Dispose on your base class.
            base.Dispose(disposing);
        }

        public override void CreateDatabaseTablesIfNotExist()
        {
            Queries.CreateMuseInputTables();
        }

        public override bool IsEnabled()
        {
            return true;
        }

        public bool MuseTrackerEnabled()
        {
            return true;
        }

        public override void Start()
        {
            Process[] proc = Process.GetProcessesByName("muse-io");
            if (proc.Length < 1)
            {
                // Run cmd command to start MuseIo if new muse process exists
                System.Diagnostics.Process.Start("CMD.exe", Settings.CmdToRunMuseIo);
            }


            // Register Save-To-Database Timer
            if (_saveToDatabaseTimer != null)
                Stop();
            _saveToDatabaseTimer = new System.Timers.Timer();
            _saveToDatabaseTimer.Interval = Settings.SaveToDatabaseInterval.TotalMilliseconds;
            _saveToDatabaseTimer.Elapsed += SaveToDatabaseTick;
            _saveToDatabaseTimer.Start();

            // Register Muse Connection Timer
            if (_checkMuseConnectionTimer != null)
                Stop();
            _checkMuseConnectionTimer = new System.Timers.Timer();
            _checkMuseConnectionTimer.Interval = Settings.CheckMuseConnectionInterval.TotalMilliseconds;
            _checkMuseConnectionTimer.Elapsed += ShowMessageWhenConnectionLost;
            _checkMuseConnectionTimer.Start();


            // SharpOSC lib from https://github.com/ValdemarOrn/SharpOSC
            // Callback function for received OSC messages. 
            HandleOscPacket callback = delegate (OscPacket packet)
            {
                var messageReceived = (OscMessage)packet;
                var addr = messageReceived.Address;

                if (messageReceived.Address != null && messageReceived.Arguments != null
                                                    && messageReceived.Arguments.Count > 0)
                {
                    SaveToBuffer(addr, messageReceived.Arguments);
                }
            };

            // Create an OSC server.
            _listener = new UDPListener(Settings.MuseIoPort, callback);

            IsRunning = true;

        }

        #region Daemon Tracker: Save to Buffer
        private static async void SaveToBuffer(String addr, List<Object> arguments)
        {
            //blink events
            if (addr == "/muse/elements/blink")
            {
                int blink = (int)arguments[0];

                if (blink == 1)
                {
                    await Task.Run(() => MuseBlinkBuffer.Enqueue(new MuseBlinkEvent(blink)));
                }
            }

            if (addr == "/muse/elements/alpha_absolute")
            {
                await Task.Run(() => MuseEEGDataBuffer.Enqueue(new MuseEEGDataEvent(MuseEEGDataType.AlphaAbsolute,
                    (float)arguments[0],
                    (float)arguments[1],
                    (float)arguments[2],
                    (float)arguments[3])));
            }

            if (addr == "/muse/elements/beta_absolute")
            {
                await Task.Run(() => MuseEEGDataBuffer.Enqueue(new MuseEEGDataEvent(MuseEEGDataType.BetaAbsolute,
                    (float)arguments[0],
                    (float)arguments[1],
                    (float)arguments[2],
                    (float)arguments[3])));
            }

            if (addr == "/muse/elements/theta_absolute")
            {
                await Task.Run(() => MuseEEGDataBuffer.Enqueue(new MuseEEGDataEvent(MuseEEGDataType.ThetaAbsolute,
                    (float)arguments[0],
                    (float)arguments[1],
                    (float)arguments[2],
                    (float)arguments[3])));
            }

            // Data quality on each channel
            if (addr == "/muse/elements/horseshoe")
            {
                if (Enum.IsDefined(typeof(MuseChannelQuality), (int)(float)arguments[0]))
                {
                    QualityLeft = (MuseChannelQuality)(int)(float)arguments[0];
                }
                else
                {
                    QualityLeft = MuseChannelQuality.Nocontact;
                }

                if (Enum.IsDefined(typeof(MuseChannelQuality), (int)(float)arguments[1]))
                {
                    QualityTopLeft = (MuseChannelQuality)(int)(float)arguments[1];
                }
                else
                {
                    QualityTopLeft = MuseChannelQuality.Nocontact;
                }

                if (Enum.IsDefined(typeof(MuseChannelQuality), (int)(float)arguments[2]))
                {
                    QualityTopRight = (MuseChannelQuality)(int)(float)arguments[2];
                }
                else
                {
                    QualityTopRight = MuseChannelQuality.Nocontact;
                }

                if (Enum.IsDefined(typeof(MuseChannelQuality), (int)(float)arguments[3]))
                {
                    QualityRight = (MuseChannelQuality)(int)(float)arguments[3];
                }
                else
                {
                    QualityRight = MuseChannelQuality.Nocontact;
                }

                Console.WriteLine("##### Data quality " + arguments[0] + arguments[1] + arguments[2] + arguments[3]);
            }

            // Muse contact 
            if (addr == "/muse/elements/touching_forehead")
            {
                museContactTsd = DateTime.Now;
                if ((int)arguments[0] == 0)
                {
                    CurrentMuseContactState = MuseContactStates.Bad;
                    Console.WriteLine("##### Muse has BAD contact");
                }else if ((int)arguments[0] == 1)
                {
                    CurrentMuseContactState = MuseContactStates.Good;
                }
            }

            // Battery level 
            if (addr == "/muse/batt")
            {
                RemainingBattery = (int)arguments[0] / 100;
            }
        }
        #endregion

        #region Daemon Tracker: Persist (save to database)

        /// <summary>
        /// Saves the buffer to the database and clears it afterwards.
        /// </summary>
        private static async void SaveToDatabaseTick(object sender, EventArgs e)
        {
            // throw and save
            await Task.Run(() => SaveInputBufferToDatabase());
        }

        /// <summary>
        /// dequeues the currently counted number of elements from the buffer and safes them to the database
        /// (it can happen that more elements are added to the end of the queue while this happens,
        /// those elements will be safed to the database in the next run of this method)
        /// </summary>
        private static void SaveInputBufferToDatabase()
        {
            DateTime Timestamp = DateTime.Now;

            try
            {
                if (MuseEEGDataBuffer.Count > 0)
                {
                    List<MuseEEGDataEvent> museData = new List<MuseEEGDataEvent>();
                    MuseEEGDataEvent museEvent = null;
                    for (var i = 0; i < MuseEEGDataBuffer.Count; i++)
                    {
                        MuseEEGDataBuffer.TryDequeue(out museEvent);
                        museData.Add(museEvent);
                    }
                    var alphaAvgChannelLeft = museData.Where(x => x.DataType == MuseEEGDataType.AlphaAbsolute && x.ChannelLeft > 0).Select(x => x.ChannelLeft).DefaultIfEmpty(0).Average();
                    var alphaAvgChannelFrontLeft = museData.Where(x => x.DataType == MuseEEGDataType.AlphaAbsolute && x.ChannelFrontLeft > 0).Select(x => x.ChannelFrontLeft).DefaultIfEmpty(0).Average();
                    var alphaAvgChannelFrontRight = museData.Where(x => x.DataType == MuseEEGDataType.AlphaAbsolute && x.ChannelFrontRight > 0).Select(x => x.ChannelFrontRight).DefaultIfEmpty(0).Average();
                    var alphaAvgChannelRight = museData.Where(x => x.DataType == MuseEEGDataType.AlphaAbsolute && x.ChannelRight > 0).Select(x => x.ChannelRight).DefaultIfEmpty(0).Average();


                    var betaAvgChannelLeft = museData.Where(x => x.DataType == MuseEEGDataType.BetaAbsolute && x.ChannelLeft > 0).Select(x => x.ChannelLeft).DefaultIfEmpty(0).Average();
                    var betaAvgChannelFrontLeft = museData.Where(x => x.DataType == MuseEEGDataType.BetaAbsolute && x.ChannelFrontLeft > 0).Select(x => x.ChannelFrontLeft).DefaultIfEmpty(0).Average();
                    var betaAvgChannelFrontRight = museData.Where(x => x.DataType == MuseEEGDataType.BetaAbsolute && x.ChannelFrontRight > 0).Select(x => x.ChannelFrontRight).DefaultIfEmpty(0).Average();
                    var betaAvgChannelRight = museData.Where(x => x.DataType == MuseEEGDataType.BetaAbsolute && x.ChannelRight > 0).Select(x => x.ChannelRight).DefaultIfEmpty(0).Average();

                    var thetaAvgChannelLeft = museData.Where(x => x.DataType == MuseEEGDataType.ThetaAbsolute && x.ChannelLeft > 0).Select(x => x.ChannelLeft).DefaultIfEmpty(0).Average();
                    var thetaAvgChannelFrontLeft = museData.Where(x => x.DataType == MuseEEGDataType.ThetaAbsolute && x.ChannelFrontLeft > 0).Select(x => x.ChannelFrontLeft).DefaultIfEmpty(0).Average();
                    var thetaAvgChannelFrontRight = museData.Where(x => x.DataType == MuseEEGDataType.ThetaAbsolute && x.ChannelFrontRight > 0).Select(x => x.ChannelFrontRight).DefaultIfEmpty(0).Average();
                    var thetaAvgChannelRight = museData.Where(x => x.DataType == MuseEEGDataType.ThetaAbsolute && x.ChannelRight > 0).Select(x => x.ChannelRight).DefaultIfEmpty(0).Average();

                    MuseEEGDataEvent[] museEEGArrayAggr = {
                        new MuseEEGDataEvent(MuseEEGDataType.AlphaAbsolute, alphaAvgChannelLeft, alphaAvgChannelFrontLeft, alphaAvgChannelFrontRight, alphaAvgChannelRight),
                        new MuseEEGDataEvent(MuseEEGDataType.BetaAbsolute, betaAvgChannelLeft, betaAvgChannelFrontLeft, betaAvgChannelFrontRight, betaAvgChannelRight),
                        new MuseEEGDataEvent(MuseEEGDataType.ThetaAbsolute, thetaAvgChannelLeft, thetaAvgChannelFrontLeft, thetaAvgChannelFrontRight, thetaAvgChannelRight)
                    };

                    Queries.SaveMuseEEGDataToDatabase(museEEGArrayAggr);
                }

                if (MuseEEGDataQualityBuffer.Count > 0)
                {
                    List<MuseEEGDataQuality> museDataQuality = new List<MuseEEGDataQuality>();
                    MuseEEGDataQuality museDataQualityEvent = null;
                    var museEEGDataQuality = new MuseEEGDataQuality[MuseEEGDataQualityBuffer.Count];

                    for (var i = 0; i < MuseEEGDataQualityBuffer.Count; i++)
                    {
                        MuseEEGDataQualityBuffer.TryDequeue(out museDataQualityEvent);
                        museDataQuality.Add(museDataQualityEvent);
                    }

                    var avgChannelLeft = museDataQuality.Average(x => x.QualityChannelLeft);
                    var avgChannelFrontLeft = museDataQuality.Average(x => x.QualityChannelFrontLeft);
                    var avgChannelFrontRight = museDataQuality.Average(x => x.QualityChannelFrontRight);
                    var avgChannelRight = museDataQuality.Average(x => x.QualityChannelRight);

                    MuseEEGDataQuality[] museEEGQualityArray = { new MuseEEGDataQuality(ZeroOrOne(avgChannelLeft), ZeroOrOne(avgChannelFrontLeft), ZeroOrOne(avgChannelFrontRight), ZeroOrOne(avgChannelRight)) };
                    Queries.SaveMuseEEGDataQualityToDatabase(museEEGDataQuality);
                }

                if (MuseBlinkBuffer.Count > 0)
                {
                    List<MuseBlinkEvent> museBlink = new List<MuseBlinkEvent>();
                    MuseBlinkEvent blinkEvent = null;
                    for (var i = 0; i < MuseBlinkBuffer.Count; i++)
                    {
                        MuseBlinkBuffer.TryDequeue(out blinkEvent);
                        museBlink.Add(blinkEvent);
                    }

                    MuseBlinkEvent[] museBlinkArrayAggr = { new MuseBlinkEvent(museBlink.Sum(x => x.Blink), Timestamp) };
                    Queries.SaveMuseBlinksToDatabase(museBlinkArrayAggr);
                }


                if (MuseConcentrationBuffer.Count > 0)
                {
                    var museConcentration = new MuseConcentrationEvent[MuseConcentrationBuffer.Count];
                    for (var i = 0; i < MuseConcentrationBuffer.Count; i++)
                    {
                        MuseConcentrationBuffer.TryDequeue(out museConcentration[i]);
                    }
                    Queries.SaveMuseConcentrationToDatabase(museConcentration);
                }


                if (MuseMellowBuffer.Count > 0)
                {
                    var museMellow = new MuseMellowEvent[MuseMellowBuffer.Count];
                    for (var i = 0; i < MuseMellowBuffer.Count; i++)
                    {
                        MuseMellowBuffer.TryDequeue(out museMellow[i]);
                    }
                    Queries.SaveMuseMellowToDatabase(museMellow);
                }

            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        #endregion

        public override void Stop()
        {
            if (_listener != null)
            {
                _listener.Close();
            }

            if (_saveToDatabaseTimer != null)
            {
                _saveToDatabaseTimer.Stop();
                _saveToDatabaseTimer.Dispose();
                _saveToDatabaseTimer = null;
            }

            IsRunning = false;

            try
            {
                Process[] proc = Process.GetProcessesByName("muse-io");
                foreach (Process p in proc)
                {
                    p.Kill();
                }
            }
            catch (Exception e)
            {
                Console.Write("### Muse: No processes to stop " + e);
            }

        }

        public override void UpdateDatabaseTables(int version)
        {
            // no database updates necessary yet
        }

        #endregion
        #endregion
        public override List<IVisualization> GetVisualizationsMonth(DateTimeOffset date)
        {
            var vis = new MonthMuseVisualization(date);
            return new List<IVisualization> { vis };
        }

        public override List<IVisualization> GetVisualizationsWeek(DateTimeOffset date)
        {
            var vis = new WeekMuseVisualization(date);

            return new List<IVisualization> { vis };
        }

        public override List<IVisualization> GetVisualizationsDay(DateTimeOffset date)
        {
            var vis2 = new DayInsightTopPgmAndMuseDataTrends(date);
            var vis3 = new DayInsightsAttentionEngagement(date);
            return new List<IVisualization> { vis2, vis3 };
        }
        /// <summary>
        /// Shows a message box when connection is lost
        /// </summary>
        private static void ShowMessageWhenConnectionLost(object sender, EventArgs e)
        {
            if (CurrentMuseContactState == MuseContactStates.Bad || CurrentMuseContactState == MuseContactStates.Nocontact)
            {
                MessageBox.Show("Muse connection gets bad contact. Try to rearrange the headband on your head.", "PersonalAnalytics Warning", MessageBoxButtons.OK);
            }
            else {
                //case that the last time it the connection was good but lost connection in the meantime
                if (DateTime.Now.Subtract(museContactTsd).TotalMinutes > 2)
                {
                    MessageBox.Show("Muse connection is lost. Please check your connection or battery state.", "PersonalAnalytics Warning", MessageBoxButtons.OK);
                }
            }
        }

    }
}
