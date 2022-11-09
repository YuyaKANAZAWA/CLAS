using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Data.GrObsData
{
    public class RawData
    {
        public string ObsCode { get; set; }
        public double Pseudorange { get; set; }
        public double CarrierPhase { get; set; }
        public double Doppler { get; set; }
        public double CN0 { get; set; }
        public bool HalfCycleAmb { get; set; }
        public double Frequency { get; set; }
        public double WabeLength { get; set; }
        public int FreqNr { get; set; }
        public int LLI_Code { get; set; }
        public int SignalStrength_Code { get; set; }
        public int LLI_Phase { get; set; }
        public int SignalStrength_Phase { get; set; }
        public string ObsCode_Code { get { return "C" + ObsCode; } }
        public string ObsCode_Phase { get { return "L" + ObsCode; } }
        public string ObsCode_Doppler { get { return "D" + ObsCode; } }
        public bool Valid_Code { get; set; }
        public bool Valid_Phase { get; set; }
        public bool Valid_Dopper { get; set; }

    }
}
