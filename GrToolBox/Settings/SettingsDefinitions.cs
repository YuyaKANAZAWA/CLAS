using System;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Settings
{
    public class SettingsDefinitions
    {
        // Positioning Mode Settings
        public enum Processing_Type
        {
            PostProcess,
            RealTimeProcess
        }

        public enum Positioning_Type
        {
            PointPositioning,
            RelativePositioning
        }

        public enum Estimation_Type
        {
            LS,
            Kalman
        }

        public enum Data_Type
        {
            RINEX,
            NMEA,
            RTCM3,
            SBF,
            UBX,
            ALL,
            UnKnown
        }


        public enum NavClock_Type
        {
            Broadcast,
            RinexNav,
            SP3,
            SP3_and_Clock
        }


        public enum Obs_Type
        {
            Rinex
        }

        public enum Ell_Type
        {
            WGS,
            ITRF
        }


        public enum Geoid_Type
        {
            EGM96
        }

        public enum Org_Type
        {
            Given,  // for debug
            Given_XYZ,
            Given_LLH,
            Spp_1st
        }

        public enum Ion_Type
        {
            None,
            Klob,
            IonFree,
            Ionex
        }

        public enum Trop_Type
        {
            None,
            Simple,
            Colins,
            Saastamoinen,
            MOPS
        }

        public enum Trop_Map
        {
            Cos_z,
            Chao,
            Neil,
        }

        public enum PcoPcv_Type
        {
            None,
            Antex,
            GSI
        }

        public enum Weighting_Type
        {
            None,
            Inv_Sin
        }


        public enum Plot_Type
        {
            EN,
            ENU_Time,
            Satellite_Time,
            SkyPlot,
            Map
        }

        public enum Connection_Type
        {
            Serial,
            TCP,
            UDP,
            Unknown
        }


        public enum Disp_Type
        {
            EN,
            ENU_Time,
            Satellite_Time,
            SkyPlot,
            Map,
            Console,
            PacketType,
            RawNmea
        }

    }
}
