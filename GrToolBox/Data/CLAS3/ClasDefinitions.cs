namespace GrToolBox.Data.CLAS3
{
    public class ClasDefinitions
    {
        public enum SubType
        {
            ST1_Mask            = 1,
            ST2_Orbit           = 2,
            ST3_Clock           = 3,
            ST4_CodeBias        = 4,
            ST5_PhaseBias       = 5,
            ST6_CodePhaseBias   = 6,
            ST7_URA             = 7,
            ST8_STEC            = 8,
            ST9_Gridded         = 9,
            ST10_ServiceInfo    = 10,
            ST11_Combined       = 11,
            ST12_Atmospheric    = 12,
            Unknown = -1,
        }

        public enum SubTypeStatus
        {
            SearchHeader,
            HeaderDone,
            DataDone,
            NeedMoreData,
            Processing,
            InvalidData
        }


    }
}
