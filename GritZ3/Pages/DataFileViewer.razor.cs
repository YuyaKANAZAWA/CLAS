using GritZ3.Classes;
using GritZ3.Components.Cards;
using GritZ3.Components.Cards.GeneralDispCard;
using GritZ3.Components.Settings;
using GrToolBox.Coordinates;
using GrToolBox.Data.Nmea;
using GrToolBox.Output;
using static GritZ3.Components.Plots.PlotDefinitions;

namespace GritZ3.Pages
{
    public partial class DataFileViewer
    {

        public DrawerInfo DataSource1Info { get; set; } = new DrawerInfo() { ID = 1, Name = "DataSource", Visible = false };
        private string[] PlotNames { get; set; } = { "Data1" };
        private string ActiveKey { get; set; } = "1";       // 表示タブ
        private List<EpochPosData> Data1 { get; set; }
        private List<EpochPosData> Data1ForPlot { get; set; }
        private DispData Data1ForCard { get; set; } = new();
        private int Data1Count { get; set; } = 0;
        public TimeSettings TimeSettings { get; set; } = new();
        //public bool E_NPlotClear = false;
        public PlotControl PlotCtrl { get; set; } = PlotControl.NoAction;

        private CancellationTokenSource? CancelSource { get; set; } = null;

        public DataFileDLCardData DataFileDLCardData { get; set; } = new DataFileDLCardData() { Title = "Convert & DL" };

        //public string TestStr { get; set; } = "NoData";

        /// <summary>
        /// Constructor
        /// </summary>
        public DataFileViewer()
        {
            DispData.TagInfo taginfo = new DispData.TagInfo() { Title = "NMEA", Color = "blue-inverse" };
            Data1ForCard.TagInfos.Add(taginfo);
            Data1ForCard.Contents.Add(new DispData.Content() { Title = "File size: ", Value = "N/A" });
            Data1ForCard.Contents.Add(new DispData.Content() { Title = "Epochs: ", Value = "N/A" });
            Data1ForCard.Contents.Add(new DispData.Content() { Title = "First epoch time: ", Value = "N/A" });
            Data1ForCard.Contents.Add(new DispData.Content() { Title = "Last epoch time: ", Value = "N/A" });
        }

        private void Open_DrawerDataSource1()
        {
            DataSource1Info.Visible = true;
        }
               
        public async void ApplySettingChange()
        {
            if (Data1ForCard == null) return;
            Data1ForCard.Title = Stg.FileName;
            var cont = Data1ForCard.Contents.FirstOrDefault(s => s.Title.Contains("File size"));
            if (cont != null) cont.Value = $"{Stg.Size:#,0} (bytes)";

            //Progress<T>オブジェクトを作成。コンストラクタ引数でイベントハンドラを渡す。
            Progress<int> progress = new Progress<int>(onProgressChanged);

            //var burstDatas = await Task.Run(() => NmeaFile.NmeaFileConverter(Stg.ServerFullPath, progress));
            var burstDatas = await Task.Run(() => NmeaFile2.NmeaFileConverter(Stg.ServerFullPath, progress));
            if(burstDatas != null)
            {
                await Task.Run(() =>
                {
                    Data1 = GetDataForPlot(burstDatas);
                    TimeSettings.Times.Clear();
                    foreach(var data in Data1)
                    {
                        TimeSettings.Times.Add(data.Time);
                    }
                });
            }
            Data1Count = Data1.Count;
            cont = Data1ForCard.Contents.FirstOrDefault(s => s.Title.Contains("Epochs"));
            if (cont != null) cont.Value = $"{Data1Count:#,0}";
            await Task.Delay(500);

            cont = Data1ForCard.Contents.FirstOrDefault(s => s.Title.Contains("First epoch time"));
            if (cont != null) cont.Value = Data1.First().Time.ToString("yyyy-MM-dd HH:mm:ss.FF");
            cont = Data1ForCard.Contents.FirstOrDefault(s => s.Title.Contains("Last epoch time"));
            if (cont != null) cont.Value = Data1.Last().Time.ToString("yyyy-MM-dd HH:mm:ss.FF");

            Data1ForPlot = Data1.GetRange(0, Data1Count - 1);

            _ = InvokeAsync(() =>
              {
                  StateHasChanged();
              });
        }

        int progress_1 = 0;
        public void onProgressChanged(int count)
        {
            //渡された達成数をプログレスバーに設定
            //Debug.WriteLine("progress!!" + (int)((double)count/(double)Stg.Size*100.0));
            progress_1 = (int)(count / (double)Stg.Size * 100.0);
            StateHasChanged();
        }

        //List<EpochPosData> Data1;
        private List<EpochPosData> GetDataForPlot(List<NmeaBurstData> burstDatas)       // TODO: OutputUtilities.NmeaBurst2EpochPosAsyncに置き換え共通化を検討すること
        {
            var epochDatas = new List<EpochPosData>();
            if (burstDatas.Count > 0)
            {
                PositionSetter PS = new PositionSetter();
                foreach (var burst in burstDatas)
                {
                    if (double.IsNaN(burst.Lat) || double.IsNaN(burst.Lon) || double.IsNaN(burst.Alt)) continue;
                    if (!PS.HaveEnuOrg) { PS.SetOrgLLH(new double[] { burst.Lat, burst.Lon, burst.Alt }, "deg"); }

                    var epoch = new EpochPosData(burst, PS);
                    epochDatas.Add(epoch);
                }
            }
            return epochDatas;
        }

        private DataFileSettings Stg { get; set; } = new();

        //DataForPlot? DataForPlot;
        //private DataForPlot GetDataForPlot(List<NmeaBurstData> burstDatas)
        //{
        //    DataForPlot data = new DataForPlot();
        //    if (burstDatas.Count > 0)
        //    {
        //        PositionSetter PS = new PositionSetter();
        //        foreach (var burst in burstDatas)
        //        {
        //            if (double.IsNaN(burst.Lat) || double.IsNaN(burst.Lon) || double.IsNaN(burst.Alt)) continue;
        //            if (!PS.HaveEnuOrg) { PS.SetOrgLLH(new double[] { burst.Lat, burst.Lon, burst.Alt }, "deg"); }

        //            data.TimeStr.Add(burst.EpochTime.ToString("O"));
        //            data.Lat.Add(burst.Lat);
        //            data.Lon.Add(burst.Lon);
        //            data.Alt.Add(burst.Alt);
        //            var pos = new Position();
        //            PS.SetLLH(pos, new double[] { burst.Lat, burst.Lon, burst.Alt }, "deg");
        //            data.East.Add(pos.Enu[0]);
        //            data.North.Add(pos.Enu[1]);
        //            data.Up.Add(pos.Enu[2]);
        //        }
        //    }
        //    return data;
        //}

        private async void OnClickPlotClear()
        {
            PlotCtrl = PlotControl.Clear;
            StateHasChanged();


            //if(TimeSettings.StartEpochInd != null && TimeSettings.EndEpochInd != null)
            //{
            //    int ss = 100;
            //    int ee = 500;
            //    //int ss = TimeSettings.StartEpochInd;
            //    //int ee = TimeSettings.EndEpochInd;
            //    //Data1ForPlot = Data1.GetRange(ss, ee);
            //    Data1ForPlot = await Task.Run(()=> Data1.GetRange(ss, ee));
            //    //await Task.Run(()=> { Data1ForPlot = Data1.GetRange(ss, ee); });
            //    //Data1ForPlot = Data1.GetRange(100, 500);
            //    E_NPlotClear = true;
            //    StateHasChanged();
            //}

        }

        private async void OnClickPlotRedraw()
        {
            //int ss = 100;
            //int ee = 500;
            int ss1 = TimeSettings.StartEpochInd;
            int ee1 = TimeSettings.EndEpochInd;
            //Data1ForPlot = Data1.GetRange(ss, ee);
            //Data1ForPlot = await Task.Run(() => Data1.GetRange(ss1, ee1 - ss1 + 1));
            await Task.Run(() => { Data1ForPlot = Data1.GetRange(ss1, ee1 - ss1 + 1); PlotCtrl = PlotControl.ReDraw; });
            //await Task.Run(()=> { Data1ForPlot = Data1.GetRange(ss, ee); });
            //Data1ForPlot = Data1.GetRange(100, 500);

            //await InvokeAsync(()=> { E_NPlotClear = true; StateHasChanged(); });
            //await InvokeAsync(()=> { E_NPlotClear = true; });
            //StateHasChanged();
            //E_NPlotClear = true;
            //await InvokeAsync(() => StateHasChanged());

        }

        private async Task OnClickPlay(bool play)
        {

            if (play)
            {
                if (CancelSource == null)
                {
                    CancelSource = new CancellationTokenSource();
                }
                var token = CancelSource.Token;

                int eStart = TimeSettings.StartEpochInd;
                int eEnd = TimeSettings.EndEpochInd;
                int ePlay = TimeSettings.PlayTimeEpochInd;

                //await Task.Run(()=> PlotCtrl = PlotControl.Clear);
                //await Task.Delay(500);

                await Task.Run(() =>
                { 
                    Data1ForPlot = Data1.GetRange(eStart, ePlay - eStart + 1); 
                    PlotCtrl = PlotControl.ReDraw;
                    //ePlay++;
                    TimeSettings.PlayTime = Data1[TimeSettings.PlayTimeEpochInd].Time;
                    //TimeSettings.PlayTimeEpochInd++;
                });

                await Task.Delay(200);

                var data = Data1.GetRange(ePlay, eEnd - ePlay + 1);
                PlotCtrl = PlotControl.Playing;
                await Task.Delay(200);


                foreach (var d in data)
                {
                    if (token.IsCancellationRequested)
                    {
                        CancelSource.Dispose();
                        CancelSource = null;
                        return;
                    }
                    TimeSettings.PlayTimeEpochInd++;
                    TimeSettings.PlayTime = d.Time;

                    Data1ForPlot = new List<EpochPosData>() { d };
                    //await InvokeAsync(()=> TestStr = d.Time.ToString());

                    StateHasChanged();
                    //await Task.Delay(500);
                    await Task.Delay(TimeSettings.PlayEpochInterval);
                }
            }
            else
            {
                if(CancelSource != null)
                {
                    CancelSource.Cancel();
                }
            }

        }

        private async void OnClickGetFile(string str)
        {
            string dirPath = "./wwwroot/";
            string kmlFileName = "kml.txt";
            string csvFileName = "csv.txt";
            string description = string.Empty;
            string fileFullPath = string.Empty;

            var epochPos = await Task.Run(() => Data1.GetRange(TimeSettings.StartEpochInd, TimeSettings.EndEpochInd - TimeSettings.StartEpochInd + 1));
            int nEpochs = epochPos.Count;

            if(str == "KML")
            {
                DataFileDLCardData.KmlFileName = kmlFileName;
                DataFileDLCardData.KmlSaveAsName = "kml.kml";
                fileFullPath = dirPath + kmlFileName;
                if (epochPos != null)
                {
                    var kml = new KmlOut();
                    await kml.WriteAsync(epochPos);
                }
            }









            if (nEpochs > 0)
            {
                long size = new FileInfo(dirPath + kmlFileName).Length;
                double sizeKB = size / 1024.0;
                await InvokeAsync(async () =>
                {
                    DataFileDLCardData.KmlDescription = $"kml.kml ({sizeKB:0.0} KB)";
                    StateHasChanged();
                });
            }

        }



        protected override async Task OnInitializedAsync()
        {
        }


    }
}
