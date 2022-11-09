using GrToolBox.Coordinates;
using GrToolBox.Time;
using System.Collections;
using System.Collections.Generic;
using static GrToolBox.Common.Constants;
using static GrToolBox.Satellite.SatelliteUtilities;

namespace GrToolBox.Satellite
{
    public class Satellites
    {
        public TimeGR Time { get; set; }

        // TOW[s], RTCMの観測データセンテンスではWnがわからないので，読み込み時はTowに読み取りエポック時刻データを格納する．測位演算の前にTimeを確定させる必要あり．
        // 時刻基準はRTCMの規定にしたがう．GPS: GPS time, GLONASS: GLONASS time
        public double Tow { get; set; } = 0.0;                      

        public List<SatelliteData> SatData { get; set; } = new List<SatelliteData>();


        public int[] NVisible { get; set; } = new int[MAX_SYS];     // システム毎の可視衛星数
        public int[] NUsed { get; set; } = new int[MAX_SYS];        // システム毎の使用衛星数
        //public BitArray SysUsed { get; private set; }
        public int[] NObsCode { get; set; } = new int[MAX_SYS];     // システム毎の擬似距離使用数
        public int[] NObsPhase { get; set; } = new int[MAX_SYS];    // システム毎の位相使用数
        public BitArray SysUsed { get; private set; } = new BitArray(MAX_SYS);



        /// <summary>
        /// Constructor
        /// </summary>
        public Satellites()
        {

        }



        public void SetEleAzi(Position pos, TimeGR timePrevious)
        {
            if (pos == null || timePrevious == null) return;    // positionデータがない，全回の測位時刻情報がない場合は，何も格納せずに返す
            //if (successTime == null || Time.Diff(successTime) > 10 * 60) return;      // TODO: 10分以内の測位結果があるとき....何分？，staticとkinematicで分けるか検討           
            double[] azel = new double[2];
            foreach (SatelliteData sat in SatData)
            {
                if (sat.Pos == null)
                {
                    continue;
                }
                AzElDcs(pos, sat.Pos, azel, sat.Dcs);
                sat.Azi = azel[0];
                sat.Ele = azel[1];
            }

        }


    }
}
