using static GrToolBox.Common.Constants;


namespace GrToolBox.Settings
{
    public class GrSettings
    {
        public PositioningMode_Setting PositioningMode { get; set; } = new ();
        public DataTypesFiles_Settings DataTypesFiles { get; set; } = new();
        public Observation_Setting Observation { get; set; } = new();
        public Satellite_Setting Satellite { get; set; } = new();
        public Correction_Setting Correction { get; set; } = new();
        public Coordinate_Setting Coordinate { get; set; } = new();
        public Communication_Settings Communication { get; set; } = new();


        //public NavClock_Setting NavClock { get; set; } = new NavClock_Setting();
        //public ObsCode_Setting ObsCode { get; set; } = new ObsCode_Setting();

        /*-----------------
          コンストラクタ 
        -----------------*/
        public GrSettings()
        {

        }

    }
}
