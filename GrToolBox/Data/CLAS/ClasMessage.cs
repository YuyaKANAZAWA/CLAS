using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Data.DataUtilities;
using GrToolBox.Data;

namespace GrToolBox.Data.CLAS
{
    public class ClasMessage
    {

        public byte Prn { get; set; }                   // PRN transmitting this message (193--211 for QZS)
        public byte VenderID { get; set; }              // 0b_101: CLAS
        public byte FacilityID { get; set; }            // 0b_00, 01: Hitachi-Ota, 0b_10, 11: Kobe
        public bool SubframeIndicator { get; set; }     // true: first data part of a subframe
        public bool AlertFlag { get; set; } = false;    // Alert Flag: true: service is not available
        public byte[] Bytes { get; set; }               // all 2000bit(250bytes) data in this CLAS message
        public int Pos { get; set; } = 49;              // next bit position in this message
        //public GrBitArray BitArray { get; set; } = new GrBitArray();    // Data Partのみのビット列（1695bit）

        /// <summary>
        /// Constructor
        /// </summary>
        public ClasMessage() { }

        public ClasMessage(byte[] b)
        {
            Bytes = b;
            Prn = b[4];
            VenderID = (byte)(b[5] >> 5);
            FacilityID = (byte)((b[5] & 0b_00011000) >> 3);
            SubframeIndicator = (b[5] & 0b_00000001) > 0;
            AlertFlag = BitToBool(b, 48);
        }

        /// <summary>
        /// Returns Reed-Solomon Code
        /// </summary>
        /// <returns></returns>
        public byte[] GetRSC()
        {
            return Bytes[218..250];
        }



    }
}
