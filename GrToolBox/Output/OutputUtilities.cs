using GrToolBox.Coordinates;
using GrToolBox.Data.Nmea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Output
{
    public class OutputUtilities
    {

        public static async Task OutputCsvAsync(List<EpochPosData> data, string path)
        {
            await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Time,Lat,Lon,Alt,EllH,East,North,Up");

                foreach (EpochPosData d in data)
                {
                    if (d != null)
                    {
                        sb.AppendLine($"{d.Time.ToString("G")},{d.Lat:.000000000},{d.Lon:.000000000},{d.Alt:F4},{d.EllH:F4},{d.East:F4},{d.North:F4},{d.Up:F4}");
                    }
                }
                File.WriteAllText(path, sb.ToString());
            });
        }



        public static List<EpochPosData> NmeaBurst2EpochPos(List<NmeaBurstData> bursts)
        {
            return NmeaBurst2EpochPos(bursts, new PositionSetter());
        }


        public static List<EpochPosData> NmeaBurst2EpochPos(List<NmeaBurstData> bursts, PositionSetter ps)
        {
            var epd = new List<EpochPosData>();
            foreach (var b in bursts)
            {
                if (double.IsNaN(b.Lat) || double.IsNaN(b.Lon) || double.IsNaN(b.Alt)) continue;
                if (!ps.HaveEnuOrg) { ps.SetOrgLLH(new double[] { b.Lat, b.Lon, b.Alt }, "deg"); }

                var epoch = new EpochPosData(b, ps);
                epd.Add(epoch);
            }
            return epd;
        }

    }
}
