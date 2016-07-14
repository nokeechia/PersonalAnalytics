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

namespace MuseTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        private UDPListener _listener;
        private string blinkFile = Settings.blinkFilePath;
        private string eegbandFile = Settings.eegbandFilePath;
        private static StreamWriter w;
        private static StreamReader sr;

        private static FileStream fs;

        private static Timer atimer;
        public Daemon()
        {
            Name = "Muse Tracker";
        }

        public override void CreateDatabaseTablesIfNotExist()
        {
            //do nothing yet
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
            //set timer interval to 1 minute
            atimer = new Timer(Settings.msTimerInterval);
            //hook up elapsed event
            atimer.Elapsed += OnTimedEvent;
            atimer.Enabled = true;

            // SharpOSC lib from https://github.com/ValdemarOrn/SharpOSC
            // Callback function for received OSC messages. 
            HandleOscPacket callback = delegate (OscPacket packet)
            {
                var messageReceived = (OscMessage)packet;
                var addr = messageReceived.Address;
                
                //blink events
                if (addr == "/muse/elements/blink")
                {
                    int blink = (int)messageReceived.Arguments[0];

                    if (blink == 1)
                    {
                        string tmp = String.Format("{0:s}", DateTime.Now);

                        if (File.Exists(blinkFile))
                        {
                            using (FileStream fs = new FileStream(Settings.blinkFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                            {
                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    sw.WriteLine(tmp);
                                }
                            }
                        }
                        Console.WriteLine("+++++" + tmp);
                    }
                }

                if (addr == "/muse/elements/alpha_absolute")
                {
                    writeToFile(Settings.alphaAbsolute, messageArgumentsToString(messageReceived.Arguments));
                }

                if (addr == "/muse/elements/beta_absolute")
                {
                    writeToFile(Settings.betaAbsolute, messageArgumentsToString(messageReceived.Arguments));
                }

                if (addr == "/muse/elements/theta_absolute")
                {
                    writeToFile(Settings.thetaAbsolute, messageArgumentsToString(messageReceived.Arguments));
                }
            };

            // Create an OSC server.
            _listener = new UDPListener(Settings.museIoPort, callback);

            IsRunning = true;
            Console.Write("++++ muse tracker started");
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            using (FileStream fss = new FileStream(Settings.eegbandFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fss))
                {
                    string lines = sr.ReadToEnd();
                    Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime + lines);
                    //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime + "nr of lines " + lines.Length);
                }
            }
        }

        private string messageArgumentsToString(List<Object> arguments) {
            string bandvalues = "";
            foreach (var arg in arguments)
            {
                bandvalues += arg + ";";
            }
            return bandvalues;
        }
        private void writeToFile(string bandname, string bandvalues)
        {
            if (File.Exists(eegbandFile))
            {

                using (FileStream fs = new FileStream(Settings.eegbandFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs)) // File.AppendText(Settings.eegbandFilePath))
                    {
                        sw.WriteLine(bandname + ";" + bandvalues + String.Format("{0:s}", DateTime.Now));
                    }
                }          
            }
        }
        public override void Stop()
        {
            if (_listener != null) {
                _listener.Close();
            }
            IsRunning = false;
            Console.Write("++++ muse tracker stopped");
        }

        public override void UpdateDatabaseTables(int version)
        {
            // no database updates necessary yet
        }
    }
}
