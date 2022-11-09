using System;
using System.Collections.Generic;
using System.Text;
using GrToolBox.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using static GrToolBox.Common.Constants;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.CommonUtilities;

namespace GrToolBox.Settings
{
    public class Satellite_Setting
    {
        [JsonIgnore]
        public PrnsBit Exclude { get; set; } = new PrnsBit();           // todo 再考

        public string[] ExcludeList { get; set; } = { "", "", "" };     // todo　再考

        public List<string> ExcludingGps { get; set; } = new List<string>();
        public List<string> ExcludingGlo { get; set; } = new List<string>();
        public List<string> ExcludingGal { get; set; } = new List<string>();
        public List<string> ExcludingQzs { get; set; } = new List<string>();
        public List<string> ExcludingBds { get; set; } = new List<string>();
        public List<string> ExcludingIrn { get; set; } = new List<string>();
        public List<string> ExcludingSbs { get; set; } = new List<string>();

        public bool ApplyElevationMask { get; set; } = true;
        public int EleMask { get; set; } = 0;   // degで指定

        public bool ApplyCnMask { get; set; } = true;
        public int[][] CnMask { get; set; } = new int[MAX_SYS][];

        public Satellite_Setting()
        {
            for(int i = 0; i< MAX_SYS; i++)
            {
                CnMask[i] = new int[3];
            }
        }

        public string[][] GetExcludingArray()
        {
            string[][] ary = new string[MAX_SYS][];
            if(ExcludingGps == null || ExcludingGps.Count == 0)
            {
                ary[GetISys(SYS.GPS)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.GPS)] = ExcludingGps.ToArray();
            }
            if(ExcludingGlo == null || ExcludingGlo.Count == 0)
            {
                ary[GetISys(SYS.GLO)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.GLO)] = ExcludingGlo.ToArray();
            }
            if(ExcludingGal == null || ExcludingGal.Count == 0)
            {
                ary[GetISys(SYS.GAL)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.GAL)] = ExcludingGal.ToArray();
            }
            if(ExcludingQzs == null || ExcludingQzs.Count == 0)
            {
                ary[GetISys(SYS.QZS)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.QZS)] = ExcludingQzs.ToArray();
            }
            if(ExcludingBds == null || ExcludingBds.Count == 0)
            {
                ary[GetISys(SYS.BDS)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.BDS)] = ExcludingBds.ToArray();
            }
            if(ExcludingIrn == null || ExcludingIrn.Count == 0)
            {
                ary[GetISys(SYS.IRN)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.IRN)] = ExcludingIrn.ToArray();
            }
            if(ExcludingSbs == null || ExcludingSbs.Count == 0)
            {
                ary[GetISys(SYS.SBS)] = new string[] { "NONE" };
            }
            else
            {
                ary[GetISys(SYS.SBS)] = ExcludingSbs.ToArray();
            }
            return ary;
        }

    }
}
