using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Settings
{
    public class DataTypesFiles_Settings
    {
        public NavClock_Type NavType { get; set; } = NavClock_Type.Broadcast;
        public Data_Type Data_Type { get; set; } = Data_Type.RINEX;
        public List<string> FnRinexNav { get; set; } = new List<string>();
        public List<string> FnSP3 { get; set; } = new List<string>();
        public List<string> FnClock { get; set; } = new List<string>();
        public string FnUsrObs { get; set; } = "";
        public string FnRefObs { get; set; } = "";

    }
}
