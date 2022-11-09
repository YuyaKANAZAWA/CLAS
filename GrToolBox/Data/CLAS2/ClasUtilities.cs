using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Common.CommonDefinitions;

namespace GrToolBox.Data.CLAS2
{
    public static class ClasUtilities
    {
        public static int GetNSat(List<ClasSSRData> ssr)
        {
            return ssr.Count;
        }

        public static int GetNSat(List<ClasSSRData> ssr, SYS sys)
        {
            return ssr.Where(d => d.GnssID == sys).Count();
        }

        public static int GetNSig(List<ClasSSRData> ssr)
        {
            int nsig = 0;
            foreach (var d in ssr)
            {
                nsig += d.IndSigs.Count;
            }
            return nsig;
        }



    }
}
