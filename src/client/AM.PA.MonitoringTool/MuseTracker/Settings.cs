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

        public const int msTimeIntervalForAggregation = 2000;
    }
}
