using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrToolBox.Data.CLAS3;

namespace GrToolBox.Output
{
    public class OSRData
    {
       
        public static void WriteLineOSR(ClasSSR ssr, StringBuilder sb)
        {   

            for(int i = 0;i<ssr.ST06.Data.Sat.Count; i++)
            {
                sb.AppendLine(
                    $"OSRRES," +
                    $"{ssr.ST01.Time.TotalSeconds}," +
                    $"{ssr.ST01.ID}," +
                    $"{0}," +
                    $"{ssr.ST06.Data.Sat[i].PhaseB[0]}," +
                    $"{ssr.ST06.Data.Sat[i].CodeB[0]}," +
                    $"" 
                         



                );
            }
 
        }

        public static void CalCodeBias(ClasSSR ssr, StringBuilder sb)
        {

        }


    }
}
