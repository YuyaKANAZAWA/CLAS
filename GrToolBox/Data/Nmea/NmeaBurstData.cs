using static GrToolBox.Data.DataTypeDefinitions;

namespace GrToolBox.Data.Nmea
{
    public class NmeaBurstData
    {
        private Nmea_Type type = Nmea_Type.Unknown;
        private DateTime epochTime;
        private TimeSpan epochTimeSpan;
        private NmeaRmc? rmc;
        private NmeaGga? gga;
        private double lat = double.NaN;
        private double lon = double.NaN;
        private double alt = double.NaN;
        private double ellHeight = double.NaN;

        public Nmea_Type Type
        {
            set { type = value; }
            get { return type; }
        }

        public DateTime EpochTime
        {
            get { return epochTime; }
        }

        public TimeSpan EpochTimeSpan
        {
            get { return epochTimeSpan; }
        }

        public NmeaRmc? Rmc
        {
            set {
                if (value == null) return;
                rmc = value;
                epochTime = rmc.Time;
                epochTimeSpan = new TimeSpan(0, rmc.Time.Hour, rmc.Time.Minute, rmc.Time.Second, rmc.Time.Millisecond);
                if(double.IsNaN(lat))
                {
                    lat = rmc.Lat;
                    lon = rmc.Lon;
                }
            }
            get { return rmc; }
        }

        public NmeaGga? Gga
        {
            set {
                if (value == null) return;
                gga = value;
                epochTimeSpan = gga.Time;
                if (double.IsNaN(alt))
                {
                    lat = gga.Lat;
                    lon = gga.Lon;
                    alt = gga.Alt;
                    ellHeight = gga.HeightEll;
                }
            }
            get { return gga; }
        }

        public bool IsTimeValid { get { return epochTime > new DateTime(1981, 1, 6, 0, 0, 0); } }
        public double Lat { get { return lat; } }
        public double Lon { get { return lon; } }
        public double Alt { get { return alt; } }
        public double EllHeight { get { return ellHeight; } }
    }

    //public class DataForPlot
    //{
    //    public List<string> TimeStr { get; set; } = new List<string>();
    //    public List<double> Lat { get; set; } = new List<double>();
    //    public List<double> Lon { get; set; } = new List<double>();
    //    public List<double> Alt { get; set; } = new List<double>();
    //    public List<double> East { get; set; } = new List<double>();
    //    public List<double> North { get; set; } = new List<double>();
    //    public List<double> Up { get; set; } = new List<double>();
    //}




}
