using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Data.Nmea.NmeaUtilities;


namespace GrToolBox.Data.Nmea
{
    public class NmeaGga
    {
        /// <summary>Raw message of GGA</summary>
        public string RawMsg { get; set; } = "";

        /// <summary>Time of day (hh,mm,ss,millisec) as timespan</summary>                
        public TimeSpan Time { get; set; }

        /// <summary>Latitude [deg]</summary>
        public double Lat { get; set; } = double.NaN;

        /// <summary>Longitude [deg]</summary>
        public double Lon { get; set; }  = double.NaN;

        /// <summary>Fix Quality</summary>
        public FixQuality Quality { get; set; } = FixQuality.Invalid;

        /// <summary>Number of satellites used (range: 0-12)</summary>
        public int NumberOfSatellites { get; set; } = 0;

        /// <summary>Horizontal Dilution of Precision</summary>
        public double Hdop { get; set; } = Double.NaN;

        /// <summary>Altitude above mean sea level [m]</summary>
        public double Alt { get; set; } = Double.NaN;

        /// <summary>
        /// Geoidal separation: the difference between the WGS-84 earth ellipsoid surface and mean-sea-level (geoid) surface
        /// equivalent to Geoid Height
        /// </summary>
        public double GeoidSeparation { get; set; } = Double.NaN;

        /// <summary>Ellipsoidal height (= alt + sep)</summary>
        public double HeightEll { get; set; } = Double.NaN;
        
        /// <summary>
        /// Age of differential corrections (zero when DGPS is not used)
        /// i.e. Time since last DGPS update
        /// </summary>
        public TimeSpan DiffAge { get; set; } = TimeSpan.Zero;

        /// <summary>ID of station providing differential corrections (null when DGPS is not used)</summary>
        public int DiffStation { get; set; } = -9999;



        //public string rawMsg { get; set; } = "";
        //public string xxGGA { get; set; } = "";             // GGA Message ID (xx = current Talker ID, see NMEA Talker IDs table)
        //public string talkerID { get; set; } = "";
        //public string rawUtc { get; set; } = "";            // UTC time
        //public double lat { get; set; } = Double.NaN;       // Latitude [deg]
        //public double lon { get; set; } = Double.NaN;       // Longitude [deg]
        //public int quality { get; set; } = -9999;           // Quality indicator for position fix
        //public int numSv { get; set; } = -9999;             // Number of satellites used (range: 0-12)
        //public double hdop { get; set; } = Double.NaN;      // Horizontal Dilution of Precision
        //public double alt { get; set; } = Double.NaN;       // Altitude above mean sea level [m]
        //public double sep { get; set; } = Double.NaN;       // Geoid separation: difference between ellipsoid and mean sea level
        //public double diffAge { get; set; } = Double.NaN;   // Age of differential corrections (null when DGPS is not used)
        //public int diffStation { get; set; } = -9999;       // ID of station providing differential corrections (null when DGPS is not used)

        //public string TimeParsed { get; set; } = "";            // Formatted UTC time (hh:mm:ss)
        ////public double HeightEll { get; set; } = Double.NaN;      // Ellipsoidal height (= alt + sep)
        //public double HeightMsl { get; set; } = Double.NaN;      // Height(mean sea lebel) (= alt)
        //public double GeoidHeight { get; set; } = Double.NaN;   // Geoid height (= sep)

        public NmeaGga(string sentence)
        {
            RawMsg = sentence;
            Parse();
        }

        //public void parseTime(String inSt)
        //{
        //    rawUtc = "";
        //    rawMsg = inSt;
        //    String[] token = rawMsg.Split(",");
        //    // (1) UTC time
        //    if (!string.IsNullOrEmpty(token[1]))
        //    {
        //        rawUtc = token[1];
        //    }
        //}

        public bool Parse()
        {
            string[] token = RawMsg.Split(",");
            string tmp;

            for (int i = 0; i < 12; i++)
            {
                if (token[i].Contains("*")) return false;
            }

            //// (0) xxGGA
            //xxGGA = token[0];
            //talkerID = xxGGA[1..3];

            // token[1]: hhmmss.ss
            if (!string.IsNullOrEmpty(token[1]))
            {
                tmp = token[1]; 
                int hh = int.Parse(tmp[0..2]);
                int mm = int.Parse(tmp[2..4]);
                double dsec = double.Parse(tmp[4..]);
                int sec = (int)dsec;
                int msec = (int)(dsec - (double)sec) * 1000;
                Time = new TimeSpan(0, hh, mm, sec, msec);
            }
            // token[2]: lat, token[3]: N/S
            if (!string.IsNullOrEmpty(token[2]) && !string.IsNullOrEmpty(token[3]))
            {
                Lat = nmeaLat2Deg(token[2], token[3]);
            }
            // (4),(5) Longitude [deg]に変換
            if (!string.IsNullOrEmpty(token[4]) && !string.IsNullOrEmpty(token[5]))
            {
                // token[4]: lon, token[5]: E/W
                Lon = nmeaLon2Deg(token[4], token[5]);
            }
            // (6) quality
            if (!string.IsNullOrEmpty(token[6]))
            {
                // token[6]: Quality indicator for position fix
                int q = int.Parse(token[6]);
                if (Enum.IsDefined(typeof(FixQuality), q))
                {
                    Quality = (FixQuality)q;
                }
                else
                {
                    Quality = FixQuality.Unknown;
                }
            }
            // token[7]: Number of satellites used (range: 0-12)
            if (!string.IsNullOrEmpty(token[7]))
            {                
                NumberOfSatellites = int.Parse(token[7]);
            }
            // token[8]: Horizontal Dilution of Precision
            if (!string.IsNullOrEmpty(token[8]))
            {                
                Hdop = double.Parse(token[8]);
            }
            // token[9]: Altitude above mean sea level [m]
            if (!string.IsNullOrEmpty(token[9]))
            {
                Alt = double.Parse(token[9]);
            }
            // token[11]: Geoid separation: difference between ellipsoid and mean sea level
            if (!string.IsNullOrEmpty(token[11]))
            {
                GeoidSeparation = double.Parse(token[11]);
            }
            // token[13]: Age of differential corrections (null when DGPS is not used)
            if (!string.IsNullOrEmpty(token[13]))
            {
                int s = (int)double.Parse(token[13]);
                DiffAge = new TimeSpan(0, 0, s);                    
            }
            // token[14]: ID of station providing differential corrections (-9999 when DGPS is not used)
            if (!string.IsNullOrEmpty(token[14]))
            {
                int iAst = token[14].IndexOf("*");
                if (iAst > 0)
                {
                    DiffStation = int.Parse(token[14][0..iAst]);
                }
            }

            //
            HeightEll = Alt + GeoidSeparation;
            //HeightMsl = alt;
            //GeoidHeight = sep;

            return true;
        }


        public enum FixQuality : int
        {
            /// <summary>Fix not available or invalid</summary>
            Invalid = 0,
            /// <summary>GPS SPS Mode, fix valid</summary>
            GpsFix = 1,
            /// <summary>Differential GPS, SPS Mode, or Satellite Based Augmentation System (SBAS), fix valid</summary>
            DgpsFix = 2,
            /// <summary>GPS PPS (Precise Positioning Service) mode, fix valid</summary>
            PpsFix = 3,
            /// <summary>Real Time Kinematic (Fixed). System used in RTK mode with fixed integers</summary>
            Rtk = 4,
            /// <summary>Real Time Kinematic (Floating). Satellite system used in RTK mode, floating integers</summary>
            FloatRtk = 5,
            /// <summary>Estimated (dead reckoning) mode</summary>
            Estimated = 6,
            /// <summary>Manual input mode</summary>
            ManualInput = 7,
            /// <summary>Simulator mode</summary>
            Simulation = 8,
            /// <summary>Simulator mode</summary>
            Unknown = 9999
        }

    }
}
