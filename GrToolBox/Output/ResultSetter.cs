using System;
using System.Collections.Generic;
using System.Text;
using GrToolBox.Coordinates;
using GrToolBox.Positioning;
using GrToolBox.Satellite;
using GrToolBox.Time;
using static GrToolBox.Common.Constants;

namespace GrToolBox.Output
{
    public static class ResultSetter
    {
        //public static PosResultGR SetPointPosResult(bool success, Position pos, List<SatelliteData> data, TimeGR time)
        public static PosResultGR SetPointPosResult(bool success, Position pos, Satellites Sats)
        {
            var result = new PosResultGR();
            
            result.Valid = success;
            result.Pos = pos;

            if(Sats.SatData.Count>0 && Sats.Time != null)
            {
                result.TimeGr = Sats.Time;
                result.TimeStr = Sats.Time.GetStringISO_DATE_TIME();
                foreach(SatelliteData Sat in Sats.SatData)
                {
                    var satdata = new SatelliteData_Base();
                    satdata.CopyValueFrom(Sat);
                    result.SatelliteList.Add(satdata);
                }
            }
            return result;
        }


        public static PosResultGR SetPointPosResult(PointPositioning_LS ppls, Satellites Sats)
        {
            var result = new PosResultGR
            {
                Valid = ppls.Success,
                Pos = ppls.EstPos
            };
            //result.RecCLK = ppls.b;
            if (result.Valid)
            {
                for (int i = 0; i < MAX_SYS; i++)
                {
                    if(Sats.NUsed[i] > 0)
                    {
                        result.RecCLK[i] = ppls.b[i] / CC;
                    }
                    else
                    {
                        result.RecCLK[i] = double.NaN;
                    }
                }
            }

            if (Sats.SatData.Count > 0 && Sats.Time != null)
            {
                result.TimeGr = Sats.Time;
                result.TimeStr = Sats.Time.GetStringISO_DATE_TIME();
                foreach (SatelliteData Sat in Sats.SatData)
                {
                    var satdata = new SatelliteData_Base();
                    satdata.CopyValueFrom(Sat);
                    result.SatelliteList.Add(satdata);
                }
            }
            return result;
        }

        public static PosResultGR SetPointPosResult(PointPositioning_KF ppkf, Satellites Sats)
        {
            var result = new PosResultGR
            {
                Valid = ppkf.Success,
                Pos = ppkf.EstPos
            };
            //result.RecCLK = ppls.b;
            if (result.Valid)
            {
                for (int i = 0; i < MAX_SYS; i++)
                {
                    if(Sats.NUsed[i] > 0)
                    {
                        result.RecCLK[i] = ppkf.Kf.Xf[3 + i] / CC;
                    }
                    else
                    {
                        result.RecCLK[i] = double.NaN;
                    }
                }
            }

            if (Sats.SatData.Count > 0 && Sats.Time != null)
            {
                result.TimeGr = Sats.Time;
                result.TimeStr = Sats.Time.GetStringISO_DATE_TIME();
                foreach (SatelliteData Sat in Sats.SatData)
                {
                    var satdata = new SatelliteData_Base();
                    satdata.CopyValueFrom(Sat);
                    result.SatelliteList.Add(satdata);
                }
            }
            return result;
        }


    }
}
