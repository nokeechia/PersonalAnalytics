using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseTracker
{
    public static class Settings
    {
        public static bool IsEnabledByDefault = true;
        public const string MuseBlinkDataTable = "museBlinkData";
        public const string MuseEEGIndexDataTable = "museEEGIndexData";

        public const int museIoPort = 5000;
        public const string blinkFilePath = @"C:\Users\seal\blinkFile.txt";
        public const string eegbandFilePath = @"C:\Users\seal\eegbandFile.txt";

        public const int msTimerInterval = 2000;//60000;

        public const string alphaAbsolute = "alpha_abs";
        public const string betaAbsolute = "beta_abs";
        public const string thetaAbsolute = "theta_abs";


        public const string DbTableMuseEEGData = "muse_input_eeg";
        public const string DbTableMuseBlink = "muse_input_blink";
        public const string DbTableMuseConcentration = "muse_input_concentration";
        public const string DbTableMuseMellow = "muse_input_mellow";

        public const string CmdToRunMuseIo = "/C muse-io --device Muse --osc osc.udp://localhost:5000";

        public const int msTimeIntervalForAggregation = 2000;

        private const int IntervalSaveToDatabaseInSeconds = 60;
        public static TimeSpan SaveToDatabaseInterval = TimeSpan.FromSeconds(IntervalSaveToDatabaseInSeconds);
    }
}
