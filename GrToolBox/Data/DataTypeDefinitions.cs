using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data
{
    public class DataTypeDefinitions
    {
        public enum Nmea_Type
        {
            GSV,
            GLL,
            VTG,
            GGA,
            GSA,
            RMC,
            ZDA,
            Unknown,
            Burst
        }



        //public enum Data_Type
        //{
        //    RINEX,
        //    NMEA,
        //    RTCM3,
        //    SBF,
        //    UBX,
        //    ALL,
        //    UnKnown
        //}

        public enum Sentence_Type
        {
            GSV,
            GLL,
            VTG,
            GGA,
            GSA,
            RMC,
            ZDA,
            Unknown
        }

        public enum NavData_Type
        {
            Brd_G,
            Brd_R,
            Brd_E,
            Brd_J,
            Brd_C,
            Brd_I,
            Brd_S,
            Sp3
        }

        public class Data_sentence
        {
            public bool Valid { get; set; } = false;
            public Data_Type Data_Type { get; set; } = Data_Type.UnKnown;
            public Sentence_Type Sentence_Type { get; set; } = Sentence_Type.Unknown;
            //public int startPos { get; set; }       // for debug
            //public int endPos { get; set; }         // for debug
            public int NextPos { get; set; }
            public int Size { get; set; } = 0;
            public string Sentence { get; set; } = "";
            public string Sentence60
            {
                get { if (Sentence.Length > 60) return Sentence[0..59]; else return Sentence; }
            }
        }



    }
}
