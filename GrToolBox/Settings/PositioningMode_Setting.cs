using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Settings
{
    public class PositioningMode_Setting
    {
        public Processing_Type Processing_Type { get; set; } = Processing_Type.PostProcess;
        public Positioning_Type PosType { get; set; } = Positioning_Type.PointPositioning;
        public Estimation_Type EstType { get; set; } = Estimation_Type.LS;



    }
}
