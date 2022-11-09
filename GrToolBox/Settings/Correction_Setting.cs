using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Settings
{
    public class Correction_Setting
    {
        public Ion_Type IonType { get; set; } = Ion_Type.None;
        public Trop_Type TropType { get; set; } = Trop_Type.None;
        public Trop_Map TropMap { get; set; } = Trop_Map.Neil;
        public PcoPcv_Type PcoPcvType { get; set; } = PcoPcv_Type.None;
        public Weighting_Type WeightingType { get; set; } = Weighting_Type.None;
        public string FnIonex { get; set; } = "";
        public string FnAntex { get; set; } = "";
        public string FnGsi { get; set; } = "";

    }
}
