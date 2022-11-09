using AntDesign;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace GritZ3.Components.Cards
{
    public partial class TimeSettingsCard
    {

        [Parameter]
        public TimeSettings TimeSettings { get; set; } = new TimeSettings();
        [Parameter]
        public EventCallback<bool> OnClickPlayPauseCallback { get; set; }
        private CultureInfo CiEn { get; } = CultureInfo.GetCultureInfo("en-US");

        private bool DisablePicker_S { get; set; } = true;
        private bool DisablePicker_E { get; set; } = true;
        private bool DisableSlider { get; set; } = true;
        private double SliderStartPos { get; set; } = 0.0;
        private double SliderEndPos { get; set; } = 100.0;
        private double SliderMin { get; set; } = 0.0;
        private double SliderMax { get; set; } = 0.0;
        private int StartEpochNum { get; set; } = 0;            // 1から始まる
        private int EndEpochNum { get; set; } = 0;

        private int EpochNumInput_Min { get; set; } = 0;
        private int EpochNumInput_Max { get; set; } = 0;

        private int DataCount { get; set; } = 0;

        private bool HaveData { get; set; } = false;

        private DateTime FirstEpoch { get; set; }
        private DateTime LastEpoch { get; set; }

        public double PlaySliderPos { get; set; } = 0.0;
        private int PlayEpochNum { get; set; } = 0;            // 1から始まる
        private bool NowPlaying { get; set; } = false;
        private string PlayTimeDisp { get; set; } = "";

        protected override async Task OnParametersSetAsync()
        {
            if(!HaveData && TimeSettings.Times.Count > 0)   // データが渡されてきたとき
            {
                await Task.Run(() =>
                {
                    DataCount = TimeSettings.Times.Count;
                    FirstEpoch = TimeSettings.Times.First();
                    LastEpoch = TimeSettings.Times.Last();
                    TimeSettings.StartTime = FirstEpoch;
                    TimeSettings.EndTime = LastEpoch;

                    EpochNumInput_Min = 1;
                    EpochNumInput_Max = DataCount;
                    StartEpochNum = 1;
                    EndEpochNum = DataCount;
                    TimeSettings.StartEpochInd = 0;
                    TimeSettings.EndEpochInd = EndEpochNum - 1;

                    SliderMin = 1.0;
                    SliderMax = (double)DataCount;
                    SliderStartPos = 1.0;
                    SliderEndPos = (double)DataCount;

                    PlaySliderPos = SliderStartPos;

                });
                await InvokeAsync(() =>
                {
                    SliderStartPos = (double)DataCount;     // 1回変化してやらないとスライダーのMAX値が反映されない
                    PlaySliderPos = SliderStartPos;
                    StateHasChanged();
                    SliderStartPos = 1.0;
                    PlaySliderPos = SliderStartPos;
                    StateHasChanged();
                });
                HaveData = true;
            }
            else
            {
                if (NowPlaying)
                {
                    PlaySliderPos = (double)TimeSettings.PlayTimeEpochInd;
                    PlayTimeDisp = TimeSettings.Times[TimeSettings.PlayTimeEpochInd].ToString("HH:mm:ss.ff");
                }
            }
        }

        private async void OnChangeStartSwitch(bool b)
        {
            TimeSettings.ChangeStart = b;
            DisablePicker_S = !b;
            DisableSlider = !b && DisablePicker_E;
            await Task.Delay(500);
            StateHasChanged();
        }

        private async void OnChangeEndSwitch(bool b)
        {
            TimeSettings.ChangeEnd = b;
            //SwitchE_Checked = b;
            DisablePicker_E = !b;
            DisableSlider = !b && DisablePicker_S;
            await Task.Delay(500);
            StateHasChanged();
        }

        private async void OnChangePicker_S(DateTimeChangedEventArgs args)
        {
            if(args.Date < FirstEpoch || args.Date > TimeSettings.EndTime)
            {
                // 範囲外・何もしない
                await Warning();
            }
            else
            {
                for(int i = 0; i < DataCount; i++)
                {
                    if(TimeSettings.Times[i] >= args.Date)
                    {
                        StartEpochNum = i + 1;
                        SliderStartPos = (double)StartEpochNum;
                        TimeSettings.StartTime = TimeSettings.Times[i];
                        TimeSettings.StartEpochInd = i;
                        break;
                    }
                }
            }
        }

        private async void OnChangePicker_E(DateTimeChangedEventArgs args)
        {
            if (args.Date < TimeSettings.StartTime || args.Date > LastEpoch)
            {
                // 範囲外・何もしない
                await Warning();
            }
            else
            {
                for (int i = DataCount - 1; i >= 0; i--)
                {
                    if (TimeSettings.Times[i] <= args.Date)
                    {
                        EndEpochNum = i + 1;
                        SliderEndPos = (double)EndEpochNum;
                        TimeSettings.EndTime = TimeSettings.Times[i];
                        TimeSettings.EndEpochInd = i;
                        break;
                    }
                }
            }
        }

        private async Task Warning()
        {
            await _message.Warning("Invalid dulation input");
        }

        private void OnSliderChange((double, double) args)
        {
            if (TimeSettings.ChangeStart)
            {
                SliderStartPos = args.Item1;
                StartEpochNum = (int)SliderStartPos;
            }
            if (TimeSettings.ChangeEnd)
            {
                SliderEndPos = args.Item2;
                EndEpochNum = (int)SliderEndPos;
            }
            int startInd = StartEpochNum - 1;
            int endInd = EndEpochNum - 1;
            TimeSettings.StartTime = TimeSettings.Times[startInd];
            TimeSettings.EndTime = TimeSettings.Times[endInd];
            TimeSettings.StartEpochInd = startInd;
            TimeSettings.EndEpochInd = endInd;

            PlaySliderPos = SliderStartPos;
            PlayEpochNum = (int)PlaySliderPos;
            int playInd = PlayEpochNum - 1;
            TimeSettings.PlayTime = TimeSettings.Times[playInd];
            TimeSettings.PlayTimeEpochInd = playInd;

            StateHasChanged();
        }


        private void OnPlayerChange((double, double) args)
        {

            PlaySliderPos = args.Item1;
            if (PlaySliderPos < SliderStartPos)
            {
                PlaySliderPos = SliderStartPos;
            }
            if(PlaySliderPos > SliderEndPos)
            {
                PlaySliderPos = SliderEndPos;
            }
            PlayEpochNum = (int)PlaySliderPos;

            int playInd = PlayEpochNum - 1;

            TimeSettings.PlayTime = TimeSettings.Times[playInd];
            TimeSettings.PlayTimeEpochInd = playInd;

            StateHasChanged();
        }



        private void OnInputNumberChanged(double value, char se)
        {
            if (se == 's')      // Start側の変更時
            {
                if (value > SliderEndPos)
                {
                    SliderStartPos = SliderEndPos;
                    SliderEndPos = value;
                }
                else
                {
                    SliderStartPos = value;
                }
                StartEpochNum = (int)SliderStartPos;
            }
            if (se == 'e')      // End側の変更時
            {
                if (value < SliderStartPos)
                {
                    SliderEndPos = SliderStartPos;
                    SliderStartPos = value;
                }
                else
                {
                    SliderEndPos = value;
                }
                EndEpochNum = (int)SliderEndPos;
            }
            int startInd = StartEpochNum - 1;
            int endInd = EndEpochNum - 1;
            TimeSettings.StartTime = TimeSettings.Times[startInd];
            TimeSettings.EndTime = TimeSettings.Times[endInd];
            TimeSettings.StartEpochInd = startInd;
            TimeSettings.EndEpochInd = endInd;
            StateHasChanged();
        }


        private void OnClickPlayPauseButton()
        {
            NowPlaying = !NowPlaying;
            OnClickPlayPauseCallback.InvokeAsync(NowPlaying);
        }

        private void IntervalChange(string value)
        {
            if(value == "1s")
            {
                TimeSettings.PlayEpochInterval = 1000;
            }
            else if(value == "500ms")
            {
                TimeSettings.PlayEpochInterval = 500;
            }
            else if(value == "100ms")
            {
                TimeSettings.PlayEpochInterval = 100;
            }
            else if(value == "50ms")
            {
                TimeSettings.PlayEpochInterval = 50;
            }
        }

    }


    public class TimeSettings
    {
        public string Title { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; } = null;
        public DateTime? EndTime { get; set; } = null;
        public List<DateTime> Times { get; set; } = new List<DateTime>();
        public int StartEpochInd { get; set; }     // start from 0(zero) 
        public int EndEpochInd { get; set; }

        public bool ChangeStart { get; set; } = false;
        public bool ChangeEnd { get; set; } = false;
        public bool ChangePlayerPos { get; set; } = false;

        public DateTime? PlayTime { get; set; } = null;
        public int PlayTimeEpochInd { get; set; }     // start from 0(zero) 

        public int PlayEpochInterval { get; set; } = 1000;

    }

}
