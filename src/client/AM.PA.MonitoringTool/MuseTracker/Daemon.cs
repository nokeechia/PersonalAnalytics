// Created by Monica Trink fom the University of Zurich
// Created: 2016-07-09
// 
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Data;
using SharpOSC;
using System.IO;

namespace MuseTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        private UDPListener _listener;
        private string blinkFile = @"C:\Users\seal\blinkFile.txt";
        private string eegbandFile = @"C:\Users\seal\eegbandFile.txt";
        private StreamWriter w;
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
            
               
                      // w = new StreamWriter(blinkFile);
            


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
                            using (StreamWriter sw = File.AppendText(blinkFile))
                            {
                                sw.WriteLine(tmp);
                            }
                        }

                        Console.WriteLine("+++++" + tmp);
                    }
                }

                if (addr == "/muse/elements/alpha_absolute")
                {
                    writeToFile("alpha_abs", messageArgumentsToString(messageReceived.Arguments));
                }

                if (addr == "/muse/elements/beta_absolute")
                {
                    writeToFile("beta_abs", messageArgumentsToString(messageReceived.Arguments));
                }

                if (addr == "/muse/elements/theta_absolute")
                {
                    writeToFile("theta_abs", messageArgumentsToString(messageReceived.Arguments));
                }
            };

            // Create an OSC server.
            _listener = new UDPListener(5000, callback);

            IsRunning = true;
            Console.Write("++++ muse tracker started");
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
                using (StreamWriter sw = File.AppendText(eegbandFile))
                {
                    sw.WriteLine(bandname + ";" + bandvalues + String.Format("{0:s}", DateTime.Now));
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
