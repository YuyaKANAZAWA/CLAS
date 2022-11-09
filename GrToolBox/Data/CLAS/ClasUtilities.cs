using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Data.CLAS
{
    public class ClasUtilities
    {
        public static int ISysCLAS_TO_ISysGR(int id)
        {
            switch (id)
            {
                case 0: return 0;       // GPS
                case 1: return 1;       // GLO
                case 2: return 2;       // GAL
                case 3: return 4;       // BDS
                case 4: return 3;       // QZS
                case 5: return 6;       // SBS
                default: return 6;      // unknown
            }
        }


    }
}
