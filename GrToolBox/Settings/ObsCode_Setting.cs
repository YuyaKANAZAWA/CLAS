using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Common.Constants;
using System.Text.Json.Serialization;
using static GrToolBox.Common.CommonUtilities;


namespace GrToolBox.Settings
{
    public class ObsCode_Setting
    {
        public string[] GPSCode { get { return Code[0]; } set { Code[0] = value; } }
        public string[] GLOCode { get { return Code[1]; } set { Code[1] = value; } }
        public string[] GALCode { get { return Code[2]; } set { Code[2] = value; } }
        public string[] QZSCode { get { return Code[3]; } set { Code[3] = value; } }
        public string[] BDSCode { get { return Code[4]; } set { Code[4] = value; } }
        public string[] IRNCode { get { return Code[5]; } set { Code[5] = value; } }
        public string[] SBSCode { get { return Code[6]; } set { Code[6] = value; } }
        public string[] GPSPhase { get { return Phase[0]; } set { Phase[0] = value; } }
        public string[] GLOPhase { get { return Phase[1]; } set { Phase[1] = value; } }
        public string[] GALPhase { get { return Phase[2]; } set { Phase[2] = value; } }
        public string[] QZSPhase { get { return Phase[3]; } set { Phase[3] = value; } }
        public string[] BDSPhase { get { return Phase[4]; } set { Phase[4] = value; } }
        public string[] IRNPhase { get { return Phase[5]; } set { Phase[5] = value; } }
        public string[] SBSPhase { get { return Phase[6]; } set { Phase[6] = value; } }

        public bool GPSL1CA { get; set; } = true;
        public bool GPSL1PY { get; set; } = false;
        public bool GPSL2PY { get; set; } = false;
        public bool GPSL2C { get; set; } = false;
        public bool GPSL5 { get; set; } = false;

        [JsonIgnore]
        public string[][] Code { get; set; } = new string[MAX_SYS][];
        [JsonIgnore]
        public string[][] Phase { get; set; } = new string[MAX_SYS][];
        [JsonIgnore]
        public string[][] Doppler { get; set; } = new string[MAX_SYS][];

        public ObsCode_Setting()
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    Code[i] = new string[MAX_SYS];
            //    Phase[i] = new string[MAX_SYS];
            //    Doppler[i] = new string[MAX_SYS];
            //    for (int j = 0; j < MAX_SYS; j++)
            //    {
            //        Code[i][j] = "---";
            //        Phase[i][j] = "---";
            //        Doppler[i][j] = "---";
            //    }
            //}

            for (int i = 0; i < 7; i++)
            {
                Code[i] = new string[3];
                Phase[i] = new string[3];
                Doppler[i] = new string[3];
                for (int j = 0; j < 3; j++)
                {
                    Code[i][j] = "---";
                    Phase[i][j] = "---";
                    Doppler[i][j] = "---";
                }
            }

        }

    }
}
