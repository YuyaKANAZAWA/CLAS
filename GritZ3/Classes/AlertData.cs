using AntDesign;

namespace GritZ3.Classes
{
    public class AlertData
    {
        public DateTime TimeData { get; set; }
        public string Type { get; set; } = AlertType.Default;
        public string Text { get; set; } = "";
        public string TextWithTime
        {
            get
            {
                if (TimeData == DateTime.MinValue)
                {
                    return Text;
                }
                else
                {
                    return TimeData.ToString("T") + " --- " + Text;
                }
            }
        }

    }
}
