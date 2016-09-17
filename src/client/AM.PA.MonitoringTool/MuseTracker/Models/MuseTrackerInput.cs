using System;


namespace MuseTracker.Models
{
    public enum MuseDataType
    {
        AlphaAbsolute,
        BetaAbsolute,
        ThetaAbsolute
    }

    class MuseEEGDataEvent : IMuseTrackerInput
    {
        public DateTime Timestamp { get; protected set; }
        public MuseDataType DataType { get; protected set; }
        public float Avg { get; protected set; }
        public float ChannelLeft { get; protected set; }
        public float ChannelFrontLeft { get; protected set; }
        public float ChannelFrontRight { get; protected set; }
        public float ChannelRight { get; protected set; }

        public MuseEEGDataEvent(MuseDataType dataType, float channelLeft, float channelFrontLeft, float channelFrontRight, float channelRight) {
            Timestamp = DateTime.Now;
            DataType = dataType;
            int count = 0;
            float sumVal = 0;

            if (channelLeft != 0)
            {
                sumVal += channelLeft;
                count++;
            }

            if (channelFrontLeft != 0)
            {
                sumVal += channelFrontLeft;
                count++;
            }

            if (channelFrontRight != 0)
            {
                sumVal += channelFrontRight;
                count++;
            }

            if (channelRight != 0)
            {
                sumVal += channelRight;
                count++;
            }

            if (sumVal != 0)
            {
                Avg = sumVal / count;
            }
            else
            {
                Avg = 0;
            }

            ChannelLeft = channelLeft;
            ChannelFrontLeft = channelFrontLeft;
            ChannelFrontRight = channelFrontRight;
            ChannelRight = channelRight;
        }
    }

    class MuseEEGDataQuality : IMuseTrackerInput
    {
        public DateTime Timestamp { get; protected set; }
        public int QualityChannelLeft { get; protected set; }
        public int QualityChannelFrontLeft { get; protected set; }
        public int QualityChannelFrontRight { get; protected set; }
        public int QualityChannelRight { get; protected set; }


        public MuseEEGDataQuality(int qualityChannelLeft, int qualityChannelFrontLeft, int qualityChannelFrontRight, int qualityChannelRight)
        {
            Timestamp = DateTime.Now;
            QualityChannelLeft = qualityChannelLeft;
            QualityChannelFrontLeft = qualityChannelFrontLeft;
            QualityChannelFrontRight = qualityChannelFrontRight;
            QualityChannelRight = qualityChannelRight;
        }
   }

    class MuseBlinkEvent : IMuseTrackerInput
    {
        public DateTime Timestamp { get; protected set; }
        public int Blink { get; protected set; }

        public MuseBlinkEvent(int blink) {
            Timestamp = DateTime.Now;
            Blink = blink;
        }
    }

    class MuseMellowEvent : IMuseTrackerInput
    {
        public DateTime Timestamp { get; protected set; }
        public float Mellow { get; protected set; }

        public MuseMellowEvent(float mellow) {
            Timestamp = DateTime.Now;
            Mellow = mellow;
        }
    }

    class MuseConcentrationEvent : IMuseTrackerInput
    {
        public DateTime Timestamp { get; protected set; }
        public float Concentration { get; protected set; }

        public MuseConcentrationEvent(float concentration) {
            Timestamp = DateTime.Now;
            Concentration = concentration;
        }
    }
}
