using GrToolBox.Coordinates;
using GrToolBox.Data.Nmea;
using GrToolBox.Output;
using static GrToolBox.Common.Constants;

namespace GritZ3.Classes
{
    public class ClassesDefinitions
    {
    }

    public class FileData
    {
        public string FileName { get; set; } = "";
        public byte[]? Content { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string ContentType { get; set; } = "";
        public long Size { get; set; }
        public string FirstEpochTime { get; set; } = "";
        public string LastEpochTime { get; set; } = "";
        public int NumberOfEpochs { get; set; }
    }

    

    public class DataForPlot
    {

        public List<string> TimeStr { get; set; } = new List<string>();
        public List<double> Lat { get; set; } = new List<double>();
        public List<double> Lon { get; set; } = new List<double>();
        public List<double> Alt { get; set; } = new List<double>();
        public List<double> EllH { get; set; } = new List<double>();
        public List<double> East { get; set; } = new List<double>();
        public List<double> North { get; set; } = new List<double>();
        public List<double> Up { get; set; } = new List<double>();
        public List<double> Hdop { get; set; } = new List<double>();
        public List<double> Vdop { get; set; } = new List<double>();


        public void Add(NmeaBurstData nmea, PositionSetter ps)
        {
            if (nmea != null && ps != null)
            {
                TimeStr.Add(nmea.EpochTime.ToString("O"));
                Lat.Add(nmea.Lat);
                Lon.Add(nmea.Lon);
                Alt.Add(nmea.Alt);
                var pos = new Position();
                ps.SetLLH(pos, new double[] { nmea.Lat, nmea.Lon, nmea.Alt }, "deg");
                East.Add(pos.Enu[0]);
                North.Add(pos.Enu[1]);
                Up.Add(pos.Enu[2]);

            }
        }
        
        
        public void Add(PosResultGR gr)
        {
            if(gr != null)
            {
                TimeStr.Add(gr.TimeGr.DT.ToString("O"));
                Lat.Add(gr.Pos.Llh[0] * RAD2DEG);
                Lon.Add(gr.Pos.Llh[1] * RAD2DEG);
                Alt.Add(gr.Pos.Alt);
                EllH.Add(gr.Pos.Llh[2]);
                East.Add(gr.Pos.Enu[0]);
                North.Add(gr.Pos.Enu[1]);
                Up.Add(gr.Pos.Enu[2]);
            }
            
        }





    }



}
