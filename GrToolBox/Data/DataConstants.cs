using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Data
{
    public static class DataConstants
    {
        /// <summary>
        /// SV accuracy (meters) look up table: see IS-GPS-200K Section 20.3.3.3.1.3;  
        /// give URA value as the index of this array
        /// </summary>
        public static readonly double[] GpsSvAccuracy = { 2.4, 3.4, 4.85, 6.85, 9.65, 13.65, 24.0, 48.0, 96.0, 192.0, 384.0, 768.0, 1536.0, 3072.0, 6144.0 };






    }
}




