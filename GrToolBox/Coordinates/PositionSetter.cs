using System;
using System.Collections.Generic;
using System.Text;
using GrToolBox.Settings;
using static GrToolBox.Coordinates.CoordinateUtilities;
using static GrToolBox.Common.Constants;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Coordinates
{
    public class PositionSetter
    {
        readonly Ell_Type EllType = Ell_Type.WGS;              // 座標系指定（WGS-84デフォルト）
        readonly Geoid_Type Geoid_Type = Geoid_Type.EGM96;      // ジオイドモデル（EGM96デフォルト）

        public bool HaveEnuOrg { get; private set; } = false;


        double[] OrgXYZ = { double.NaN, double.NaN, double.NaN };
        double[] OrgLLH = { double.NaN, double.NaN, double.NaN };

        GeoidModel Gm { get; set; }

        /*----------------
            Constructor
        -----------------*/
        public PositionSetter(Ell_Type ell_Type, Geoid_Type geoid_Type)
        {
            this.EllType = ell_Type;
            this.Geoid_Type = geoid_Type;
            this.Gm = new GeoidModel(geoid_Type);
        }

        public PositionSetter(): this(Ell_Type.WGS, Geoid_Type.EGM96){ }                    //  両方指定無し

        public PositionSetter(Ell_Type ell_Type) : this(ell_Type, Geoid_Type.EGM96) { }     // 楕円体のみ指定
        public PositionSetter(Geoid_Type geoid_Type) : this(Ell_Type.WGS, geoid_Type) { }   // ジオイドモデルのみ指定
        public PositionSetter(Coordinate_Setting stg): this(stg.EllType, stg.GeoidType) { } // Settingsからの設定
        public PositionSetter(Settings.GrSettings stg) : this(stg.Coordinate) { }

        /*----------------
            Public methods
        -----------------*/
        public void SetOrgXYZ(double[] xyz)
        {
            OrgXYZ[0] = xyz[0];
            OrgXYZ[1] = xyz[1];
            OrgXYZ[2] = xyz[2];
            OrgLLH = XYZ2llh(OrgXYZ, EllType);
            HaveEnuOrg = true;
        }

        public void SetOrgLLH(double[] llh, params string[] args)
        {
            if ((args.Length == 1) && (args[0] == "deg"))
            {
                OrgLLH[0] = llh[0] * DEG2RAD;
                OrgLLH[1] = llh[1] * DEG2RAD;
                OrgLLH[2] = llh[2];
            }
            else
            {
                OrgLLH[0] = llh[0];
                OrgLLH[1] = llh[1];
                OrgLLH[2] = llh[2];
            }
            OrgXYZ = LLH2xyz(OrgLLH, EllType);
            HaveEnuOrg = true;
        }

        public void SetXYZ(Position pos, double[] xyz)
        {
            pos.Xyz[0] = xyz[0];
            pos.Xyz[1] = xyz[1];
            pos.Xyz[2] = xyz[2];
            pos.Llh = XYZ2llh(xyz, EllType);
            pos.GeoidHeight = Gm.GetGeoidHeight(pos.Llh[0] * RAD2DEG, pos.Llh[1] * RAD2DEG);
            pos.Alt = pos.Llh[2] - pos.GeoidHeight;
            if (HaveEnuOrg)
            {
                pos.Enu = XYZ2enu(pos.Xyz, OrgXYZ, OrgLLH, EllType);
            }
        }

        public void SetLLH(Position pos, double[] llh, params string[] args)
        {
            if ((args.Length == 1) && (args[0] == "deg"))
            {
                pos.Llh[0] = llh[0] * DEG2RAD;
                pos.Llh[1] = llh[1] * DEG2RAD;
                pos.Llh[2] = llh[2];
            }
            else
            {
                pos.Llh[0] = llh[0];
                pos.Llh[1] = llh[1];
                pos.Llh[2] = llh[2];
            }
            pos.Xyz = LLH2xyz(pos.Llh, EllType);
            pos.GeoidHeight = Gm.GetGeoidHeight(pos.Llh[0] * RAD2DEG, pos.Llh[1] * RAD2DEG);
            pos.Alt = pos.Llh[2] - pos.GeoidHeight;
            if (HaveEnuOrg)
            {
                pos.Enu = XYZ2enu(pos.Xyz, OrgXYZ, OrgLLH, EllType);
            }
        }

        public void SetENU(Position pos, double[] enu)
        {
            pos.Enu[0] = enu[0];
            pos.Enu[1] = enu[1];
            pos.Enu[2] = enu[2];
            if (HaveEnuOrg)
            {
                pos.Xyz = ENU2xyz(enu, OrgXYZ, EllType);
                pos.Llh = XYZ2llh(pos.Xyz, EllType);
                pos.GeoidHeight = Gm.GetGeoidHeight(pos.Llh[0] * RAD2DEG, pos.Llh[1] * RAD2DEG);
                pos.Alt = pos.Llh[2] - pos.GeoidHeight;
            }
        }
    }
}




