using System;
using System.Collections.Generic;
using System.Text;
using GrToolBox.Common;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Settings
{
    public class NavClock_Setting
    {
        public NavClock_Type Type { get; set; } = NavClock_Type.RinexNav;
        public List<string> FnRinexNav { get; set; } = new List<string>();
        public List<string> FnSP3 { get; set; } = new List<string>();
        public List<string> FnClock { get; set; } = new List<string>();

    }
}
