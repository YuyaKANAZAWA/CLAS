using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Data.CLAS2.ClasDefinitions;

namespace GrToolBox.Data.CLAS2
{
    public class ST9_Gridded : ClasHeaderCommonData
    {
        // additional header
        public int TropType { get; set; }
        public int Nbits_STEC { get; set; }
        public int NetworkID { get; set; }                         // Compact Network ID
        private ulong NetSVMask { get; set; }                       // Compact Network SV Mask

        public int TropQI_U { get; set; }                           // Troposphere Quality Indicator, bits 5-3
        public int TropQI_L { get; set; }                           // Troposphere Quality Indicator, bits 2-0
        public int NGrids { get; set; }

        private ST01_Mask ST01 { get; set; }

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST5", "Clas Subtype5");

        public ST9_Gridded(ClasHeaderCommonData h, ST01_Mask st01)
        {
            TraceSwitch.Level = TraceLevel.Verbose;
            if (h != null && st01 != null)
            {
                AddHeaderData(h);
                ST01 = st01;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            MessageNumber = h.MessageNumber;
            ID = h.ID;
            Time = h.Time;
            UpdateInterval = h.UpdateInterval;
            Multiple = h.Multiple;
            IodSsr = h.IodSsr;
        }


        //public SubTypeStatus Decode(BitCircularBuffer bcb)
        //{
        //    // read additional header


        //}


    }
}
