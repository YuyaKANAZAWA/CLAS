using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Settings.SettingsDefinitions;
using GrToolBox.Coordinates;

namespace GrToolBox.Settings
{
    public class Coordinate_Setting
    {
        public Org_Type EnuOrgType { get; set; } = Org_Type.Spp_1st;
        public Org_Type RefPosType { get; set; } = Org_Type.Spp_1st;
        public Ell_Type EllType { get; set; } = Ell_Type.WGS;
        public Geoid_Type GeoidType { get; set; } = Geoid_Type.EGM96;
        public Position EnuOrg { get; set; } = null;
        public Position RefPos { get; set; } = null;

    }
}
