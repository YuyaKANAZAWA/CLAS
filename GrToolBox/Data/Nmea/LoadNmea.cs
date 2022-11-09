using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using GrToolBox.Coordinates;
using GrToolBox.Output;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.Nmea.NmeaUtilities;
using GrToolBox.Satellite;
using static GrToolBox.Satellite.SatelliteDefinitions;
using GrToolBox.Time;

namespace GrToolBox.Nmea
{
    public class LoadNmea
    {
    //    public static List<PosResult> Nmea2Result(string fn)
    //    {
    //        bool haveOrgEnu = false;
    //        PositionSetter ps = new PositionSetter();
    //        List<PosResult> Result = new List<PosResult>();

    //        byte[] Data = File.ReadAllBytes(fn);
    //        Nmea nm = new Nmea(Data);
    //        bool next = true;
    //        while (next)
    //        {
    //            switch (nm.haveNext())
    //            {
    //                case 0:
    //                    // 読み込み終了
    //                    next = false;
    //                    break;
    //                case 1:         //  storeしていく
    //                    PosResult res = new PosResult();
    //                    Position pos = new Position();
    //                    string ggaTime = "";
    //                    string zdaTime = "";
    //                    if (nm.GGA.Count > 0)
    //                    {
    //                        foreach (NmeaGga gga in nm.GGA)
    //                        {
    //                            if (gga.rawUtc == nm.timeTobeProcessed)
    //                            {
    //                                if (!haveOrgEnu)
    //                                {
    //                                    ps.SetOrgLLH(new double[] { gga.lat, gga.lon, gga.HeightEll }, "deg");
    //                                    haveOrgEnu = true;
    //                                }
    //                                ggaTime = gga.TimeParsed;
    //                                ps.SetLLH(pos, new double[] { gga.lat, gga.lon, gga.HeightEll }, "deg");
    //                                res.Pos = pos;
    //                                res.Dop.H = gga.hdop;
    //                            }
    //                        }
    //                        foreach (NmeaZda zda in nm.ZDA)
    //                        {
    //                            if (zda.rawUtc == nm.timeTobeProcessed)
    //                            {
    //                                zdaTime = zda.CompleteTime;
    //                            }
    //                        }
    //                        if (!string.IsNullOrEmpty(zdaTime))
    //                        {
    //                            res.TimeStr = zdaTime;
    //                        }
    //                        else
    //                        {
    //                            res.TimeStr = ggaTime;
    //                        }

    //                        //for each GSV
    //                        foreach(NmeaGsv gsv in nm.GSV)
    //                        {
    //                            int ns = gsv.numSVinThisSentence;
    //                            for(int i=0; i<ns; i++)
    //                            {
    //                                int prn = gsv.prnGR[i];
    //                                NmeaSatelliteData sat = res.Sat.FirstOrDefault(s => s.Prn == prn);  // prnが一致するSatelliteDataを探す
    //                                if (sat == null)
    //                                {
    //                                    sat = new NmeaSatelliteData();
    //                                    sat.Prn = prn;
    //                                    sat.SvIdNmea = gsv.svid[i];
    //                                    res.Sat.Add(sat);
    //                                }

    //                                if (gsv.elv[i]>=0 && gsv.az[i] >= 0)
    //                                {
    //                                    sat.Ele = gsv.elv[i] * DEG2RAD;
    //                                    sat.Azi = gsv.az[i] * DEG2RAD;
    //                                }

    //                                if(gsv.cn0[i] != -9999)
    //                                {
    //                                    int band = NmeaSigID2L1_2_5(gsv.talkerID, gsv.signalID);
    //                                    sat.Cn0[band] = gsv.cn0[i];
    //                                }
    //                            }
    //                        }

    //                        //for each GSA
    //                        foreach(NmeaGsa gsa in nm.GSA)
    //                        {
    //                            for(int i=0; i<12; i++)
    //                            {
    //                                int prn = gsa.PrnGR[i];
    //                                if(prn != -9999)
    //                                {
    //                                    NmeaSatelliteData sat = res.Sat.FirstOrDefault(s => s.Prn == prn);  // prnが一致するSatelliteDataを探す
    //                                    if (sat == null)
    //                                    {
    //                                        sat = new NmeaSatelliteData();
    //                                        sat.Prn = prn;
    //                                        sat.SvIdNmea = gsa.svid[i];
    //                                        res.Sat.Add(sat);
    //                                    }
    //                                    sat.InUse = true;
    //                                    res.Dop.P = gsa.PDOP;
    //                                    res.Dop.H = gsa.HDOP;
    //                                    res.Dop.V = gsa.VDOP;
    //                                }
    //                            }
    //                        }

    //                        foreach(NmeaSatelliteData s in res.Sat)
    //                        {
    //                            if (!double.IsNaN(s.Cn0[0])) s.Status += 1;
    //                            if (!double.IsNaN(s.Cn0[1])) s.Status += 2;
    //                            if (!double.IsNaN(s.Cn0[2])) s.Status += 4;

    //                        }



    //                        Result.Add(res);
    //                    }
    //                    goto case 2;
    //                case 2:
    //                    nm.reset();     // storeしたデータをリセット
    //                    goto case 3;
    //                case 3:
    //                    nm.parse();     // 読む
    //                    break;
    //            }
    //        }
    //        return Result;
    //    }

    //    public static List<string> NmeaRawMsg(string fn)
    //    {
    //        List<string> rawMsg = new List<string>();
    //        byte[] Data = File.ReadAllBytes(fn);
    //        Nmea nm = new Nmea(Data);
    //        bool next = true;
    //        while (next)
    //        {
    //            switch (nm.haveNext())
    //            {
    //                case 0:
    //                    // 読み込み終了
    //                    next = false;
    //                    break;
    //                default:
    //                    rawMsg.Add(nm.sentence);
    //                    break;
    //            }
    //        }
    //        return rawMsg;
    //    }










    //    public static List<PosResultGR> Nmea2Result_rev(Nmea_rev nmea)
    //    {
    //        bool haveOrgEnu = false;
    //        PositionSetter ps = new PositionSetter();
    //        List<PosResultGR> Results = new List<PosResultGR>();

    //        //byte[] Data = File.ReadAllBytes(fn);
    //        //Nmea nm = new Nmea(Data);
    //        DateTime day_first_epoch = DateTime.Today;
    //        DateTime time_this_epoch;
    //        DateTime time_previous_epoch = DateTime.MinValue;
    //        DateTime zdaDay = DateTime.Today;

    //        bool next = true;
    //        while (next)
    //        {
    //            switch (nmea.NextSentence())
    //            {
    //                case 0:
    //                    // 読み込み終了
    //                    next = false;
    //                    break;
    //                case 1:         //  storeしていく
    //                    if(nmea.HaveZDA && nmea.ZDA.Count==0)
    //                    {
    //                        continue;
    //                    }
    //                    PosResultGR res = new PosResultGR();
    //                    Position pos = new Position();
    //                    string ggaTime = "";
    //                    string zdaTime = "";
    //                    if (nmea.GGA.Count > 0)
    //                    {
    //                        foreach (NmeaGga gga in nmea.GGA)
    //                        {
    //                            if (gga.rawUtc == nmea.TimeTobeProcessed)
    //                            {
    //                                //if (!haveOrgEnu)
    //                                if (!haveOrgEnu && !Double.IsNaN(gga.lat) && !Double.IsNaN(gga.lon) && !Double.IsNaN(gga.HeightEll))
    //                                {
    //                                    ps.SetOrgLLH(new double[] { gga.lat, gga.lon, gga.HeightEll }, "deg");
    //                                    haveOrgEnu = true;
    //                                }
    //                                ggaTime = gga.TimeParsed;
    //                                ps.SetLLH(pos, new double[] { gga.lat, gga.lon, gga.HeightEll }, "deg");
    //                                res.Pos = pos;
    //                                res.Dop.H = gga.hdop;     //todo DOP
    //                            }
    //                        }

                            

    //                        foreach (NmeaZda zda in nmea.ZDA)
    //                        {
    //                            if (zda.rawUtc == nmea.TimeTobeProcessed)
    //                            {
    //                                zdaTime = zda.CompleteTime;
    //                                zdaDay = new DateTime(zda.year, zda.month, zda.day);
    //                            }
    //                        }
    //                        if (!string.IsNullOrEmpty(zdaTime))
    //                        {
    //                            res.TimeStr = zdaTime;
    //                            if (Results.Count == 0)
    //                            {
    //                                //day_first_epoch = new DateTime(int.Parse(res.TimeStr[0..4]), int.Parse(res.TimeStr[5..7]), int.Parse(res.TimeStr[8..10]));  // TODO: 日付が変わると怪しいのでチェックする！！
    //                                day_first_epoch = zdaDay;
    //                            }

    //                        }
    //                        else
    //                        {
    //                            res.TimeStr = ggaTime;
    //                        }

    //                        time_this_epoch = day_first_epoch + new TimeSpan(0, int.Parse(ggaTime[0..2]), int.Parse(ggaTime[3..5]), int.Parse(ggaTime[6..8]), int.Parse(ggaTime[9..11]) * 100);
    //                        res.TimeGr = new TimeGR(time_this_epoch);

    //                        //for each GSV
    //                        foreach (NmeaGsv gsv in nmea.GSV)
    //                        {
    //                            int ns = gsv.numSVinThisSentence;
    //                            for (int i = 0; i < ns; i++)
    //                            {
    //                                int prn = gsv.prnGR[i];
    //                                if(prn == -9999)
    //                                {
    //                                    continue;
    //                                }
    //                                SatelliteData_Base sat = res.SatelliteList.FirstOrDefault(s => s.Prn == prn);  // prnが一致するSatelliteDataを探す
    //                                if (sat == null)
    //                                {
    //                                    sat = new SatelliteData_Base(prn);
    //                                    //sat.Prn = prn;
    //                                    //sat.SvIdNmea = gsv.svid[i];
    //                                    sat.Status = 0x001 << (int)Status_Type.Unknown;     // GSV=可視衛星．これだけでは使用しているか分からないのでUnknownとする.GSAにエントリーあれば使用しているので，GSAの処理内で書き換える
    //                                    res.SatelliteList.Add(sat);
    //                                }

    //                                if (gsv.elv[i] >= 0 && gsv.az[i] >= 0)
    //                                {
    //                                    sat.Ele = gsv.elv[i] * DEG2RAD;
    //                                    sat.Azi = gsv.az[i] * DEG2RAD;
    //                                }

    //                                if (gsv.cn0[i] != -9999)
    //                                {
    //                                    int band = NmeaSigID2L1_2_5(gsv.talkerID, gsv.signalID);
    //                                    sat.Cn0[band] = gsv.cn0[i];
    //                                }
    //                            }
    //                        }

    //                        //for each GSA
    //                        foreach (NmeaGsa gsa in nmea.GSA)
    //                        {
    //                            for (int i = 0; i < 12; i++)
    //                            {
    //                                int prn = gsa.PrnGR[i];
    //                                if (prn != -9999)
    //                                {
    //                                    SatelliteData_Base sat = res.SatelliteList.FirstOrDefault(s => s.Prn == prn);  // prnが一致するSatelliteDataを探す
    //                                    if (sat == null)
    //                                    {
    //                                        sat = new SatelliteData_Base();
    //                                        //sat.Prn = prn;
    //                                        //sat.SvIdNmea = gsa.svid[i];
    //                                        res.SatelliteList.Add(sat);
    //                                    }
    //                                    sat.Status = 0;     // GSAにエントリ=測位に使用．Status=0
    //                                    res.Dop.P = gsa.PDOP;
    //                                    res.Dop.H = gsa.HDOP;
    //                                    res.Dop.V = gsa.VDOP;
    //                                }
    //                            }
    //                        }

    //                        foreach (SatelliteData_Base s in res.SatelliteList)
    //                        {
    //                            if (!double.IsNaN(s.Cn0[0])) s.HaveCodeData += 1;
    //                            if (!double.IsNaN(s.Cn0[1])) s.HaveCodeData += 2;
    //                            if (!double.IsNaN(s.Cn0[2])) s.HaveCodeData += 4;

    //                        }
    //                        Results.Add(res);
    //                    }
    //                    goto case 2;
    //                case 2:
    //                    nmea.Reset();     // storeしたデータをリセット
    //                    goto case 3;
    //                case 3:
    //                    nmea.Parse();     // 読む
    //                    break;
    //            }
    //        }
    //        return Results;
    //    }
    }
}
