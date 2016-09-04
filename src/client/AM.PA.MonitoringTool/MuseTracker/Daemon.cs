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

namespace MuseTracker
{
    public class Daemon : BaseTrackerDisposable, ITracker
    {
        #region FIELDS
        private bool _disposed = false;
        private Timer _saveToDatabaseTimer;
        private UDPListener _listener;
        //private string _blinkFile = Settings.blinkFilePath;
        //private string _eegbandFile = Settings.eegbandFilePath;
        //private static StreamWriter w;
        //private static StreamReader sr;

        // buffers for user input, they are emptied every 60s (Settings.IntervalSaveToDatabaseInSeconds)
        private static readonly ConcurrentQueue<MuseEEGDataEvent> MuseEEGDataBuffer = new ConcurrentQueue<MuseEEGDataEvent>();
        private static readonly ConcurrentQueue<MuseBlinkEvent> MuseBlinkBuffer = new ConcurrentQueue<MuseBlinkEvent>();
        private static readonly ConcurrentQueue<MuseConcentrationEvent> MuseConcentrationBuffer = new ConcurrentQueue<MuseConcentrationEvent>();
        private static readonly ConcurrentQueue<MuseMellowEvent> MuseMellowBuffer = new ConcurrentQueue<MuseMellowEvent>();

        //private static FileStream fs;
        //private static Timer atimer;

        #endregion
        
        #region METHODS
        
        #region ITracker Stuff

        public Daemon()
        {
            Name = "Muse Tracker";
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _saveToDatabaseTimer.Dispose();
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

        public bool MuseTrackerEnabled() {
            return true;
        }

        public override void Start()
        {
            // Register Save-To-Database Timer
            if (_saveToDatabaseTimer != null)
                Stop();
            _saveToDatabaseTimer = new Timer();
            _saveToDatabaseTimer.Interval = Settings.SaveToDatabaseInterval.TotalMilliseconds;
            _saveToDatabaseTimer.Elapsed += SaveToDatabaseTick;
            _saveToDatabaseTimer.Start();


            //set timer interval to 1 minute
            //atimer = new Timer(Settings.msTimerInterval);
            //hook up elapsed event
            //atimer.Elapsed += OnTimedEvent;
            //atimer.Enabled = true;

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
            _listener = new UDPListener(Settings.museIoPort, callback);

            IsRunning = true;
            Console.Write("++++ muse tracker started");

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
                    //Console.Write("##### Blink event");
                    await Task.Run(() => MuseBlinkBuffer.Enqueue(new MuseBlinkEvent(blink)));
                }
            }

            if (addr == "/muse/elements/alpha_absolute")
            {
                //Console.Write("##### Alpha abs value");
                await Task.Run(() => MuseEEGDataBuffer.Enqueue(new MuseEEGDataEvent(MuseDataType.AlphaAbsolute,
                    (float) arguments[0],
                    (float) arguments[1],
                    (float) arguments[2],
                    (float) arguments[3])));
            }

            if (addr == "/muse/elements/beta_absolute")
            {
               // Console.Write("##### Beta abs value");
                await Task.Run(() => MuseEEGDataBuffer.Enqueue(new MuseEEGDataEvent(MuseDataType.BetaAbsolute,
                    (float)arguments[0],
                    (float)arguments[1],
                    (float)arguments[2],
                    (float)arguments[3])));
            }

            if (addr == "/muse/elements/theta_absolute")
            {
                //Console.Write("##### Theta abs value");
                await Task.Run(() => MuseEEGDataBuffer.Enqueue(new MuseEEGDataEvent(MuseDataType.ThetaAbsolute,
                    (float)arguments[0],
                    (float)arguments[1],
                    (float)arguments[2],
                    (float)arguments[3])));
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
        private static void SaveInputBufferToDatabase() {
            Console.Write("Save Input Buffer to DB: EEG, Blink, Concent, Mellow" + MuseEEGDataBuffer.Count + " " + MuseBlinkBuffer.Count
                + " " + MuseConcentrationBuffer.Count + " " + MuseMellowBuffer.Count);
            try
            {
                if (MuseEEGDataBuffer.Count > 0)
                {
                    var museEEGData = new MuseEEGDataEvent[MuseEEGDataBuffer.Count];
                    for (var i = 0; i < MuseEEGDataBuffer.Count; i++)
                    {
                        MuseEEGDataBuffer.TryDequeue(out museEEGData[i]);
                    }
                    Queries.SaveMuseEEGDataToDatabase(museEEGData);
                }

                if (MuseBlinkBuffer.Count > 0)
                {
                    var museBlink = new MuseBlinkEvent[MuseBlinkBuffer.Count];
                    for (var i = 0; i < MuseBlinkBuffer.Count; i++)
                    {
                        MuseBlinkBuffer.TryDequeue(out museBlink[i]);
                    }
                    Queries.SaveMuseBlinksToDatabase(museBlink);
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
            if (_listener != null) {
                _listener.Close();
            }

            if (_saveToDatabaseTimer != null)
            {
                _saveToDatabaseTimer.Stop();
                _saveToDatabaseTimer.Dispose();
                _saveToDatabaseTimer = null;
            }

            IsRunning = false;
            Console.Write("++++ muse tracker stopped");
        }

        public override void UpdateDatabaseTables(int version)
        {
            // no database updates necessary yet
        }

        #endregion
        #endregion
        public override List<IVisualization> GetVisualizationsMonth(DateTimeOffset date)
        {
            var vis1 = new MonthMuseAttentionVisualization(date);
            var vis2 = new MonthMuseEngagementVisualization(date);
            return new List<IVisualization> { vis1, vis2 };
        }
    }
}
