using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using System.Text;
using GrToolBox.Data.CLAS3;
using static GrToolBox.Data.CLAS3.ClasFile;
using static GrToolBox.Data.CLAS3.ClasSSR;
using static GrToolBox.Common.Constants;
using static GrToolBox.Settings.SettingsDefinitions;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GrToolBox.Data.SBF
{
    public class QZSRawL6_4069 : Header_TimeStamp, IRawData
    {
        private byte SVID { get; set; }        // Satellite ID, see 4.1.9
        private byte Parity { get; set; }     // Status of the Reed-Solomon decoding:
                                              // 0: Failed: unrecoverable errors found. There is at least one wrong bit in NavBits.
                                              // 1: Passed: all bit errors could be recovered, or the message wasreceived without bit error.
        private byte RSCnt { get; set; }         // Number of symbol errors that were successfully corrected by the Reed-Solomon decoder.
        private byte Source { get; set; }        // Source of the message:
                                                 // 0: Unknown
                                                 // 1: QZSS L6D
                                                 // 2: QZSS L6E
        private byte Reserved { get; set; }        // Data flag for L2 P-code (1 bit from subframe 1, word 4)
        private ushort RxChannel { get; set; }     // Receiver channel (see 4.1.11).
        

        private byte[] NAVBits { get; set; }      // NAVBits contains the 2000 bits of a QZSS L6 message.
                                                  // Encoding: NAVBits contains all the bits of the message after Reed-Solomon decoding,
                                                  // including the preamble and the Reed-Solomonparity symbols themselves.
                                                  // The first received bit is stored as the MSB of NAVBits[0].
                                                  // The unused bits in NAVBits[63] must be ignored by the decoding software.

        public List<ClasMessage> clasMessage { get; set; }
        public ClasMessage ClasMessages { get; set; }


        bool yn { get; set; }


        public QZSRawL6_4069() { }

        public QZSRawL6_4069(byte[] byteData, Header_TimeStamp h)
        {
            Decode(byteData, h);
        }

        public int GetID()
        {
            return 4069;
        }

        public Satellites? GetMeas()
        {
            return null;
        }

        public GrNavBase? GetNav()
        {
            return SBF2CLAS();
        }

        public void PrintTo(StringBuilder sb) { }

        private void Decode(byte[] byteData, Header_TimeStamp h)
        {
            int pos = 14;
            byte svid = byteData[pos]; pos++;
            byte parity = byteData[pos]; pos++;
            byte rs_cnt = byteData[pos]; pos++;
            byte source = byteData[pos]; pos++;
            byte reserved = byteData[pos]; pos++;
            byte rx_channel = byteData[pos]; pos++;

            byte[] nav_bits_little = byteData[pos..(pos + 4 * 63)];
            pos += 4 * 63;

            byte[] nav_bits_big = new byte[252];

            for (int i = 0; i < 63; i++)
            {
                nav_bits_big[4 * i] = nav_bits_little[4 * i + 3];
                nav_bits_big[4 * i + 1] = nav_bits_little[4 * i + 2];
                nav_bits_big[4 * i + 2] = nav_bits_little[4 * i + 1];
                nav_bits_big[4 * i + 3] = nav_bits_little[4 * i];
            }

            Sync1 = h.Sync1;
            Sync2 = h.Sync2;
            BlockNum = h.BlockNum;
            BlockRev = h.BlockRev;
            Length = h.Length;
            Tow = h.Tow;
            WNc = h.WNc;

            SVID = svid;
            Parity = parity;
            RSCnt = rs_cnt;
            Source = source;
            Reserved = reserved;
            RxChannel = rx_channel;

            clasMessage = ClasFileConverter(nav_bits_big);
            ClasMessages = clasMessage[0];

        }
        private ClasMessage SBF2CLAS()
        {

            ClasMessage clas = new ClasMessage();

            clas.Prn = ClasMessages.Prn;
            clas.VenderID = ClasMessages.VenderID;
            clas.FacilityID = ClasMessages.FacilityID;
            clas.SubframeIndicator = ClasMessages.SubframeIndicator;
            clas.AlertFlag = ClasMessages.AlertFlag;
            clas.Bytes = ClasMessages.Bytes;

            return clas;

        }


    }
}
