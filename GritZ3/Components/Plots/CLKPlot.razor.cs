using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;
using static GrToolBox.Common.Constants;

namespace GritZ3.Components.Plots
{
    public partial class CLKPlot
    {
        [Parameter]
        public string[]? PlotNames { get; set; }  
        [Parameter]
        public List<EpochPosData?>? Data1 { get; set; }    // LS 用
        [Parameter]
        public List<EpochPosData?>? Data2 { get; set; }    // KF 用
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }
        [Parameter]
        public bool FilePlot { get; set; }

        private bool HaveRendered { get; set; } = false;   // fileデータプロット時の描画制御用


        PlotlyChart? chart_LS;
        PlotlyChart? chart_KF;
        Config config_LS = new Config() { Responsive = true };
        Config config_KF = new Config() { Responsive = true };
        Plotly.Blazor.Layout layout_LS = new();
        Plotly.Blazor.Layout layout_KF = new();

        // Using of the interface IList is important for the event callback!
        IList<ITrace> data_LS;
        IList<ITrace> data_KF;

        private void GenerateLayout(Plotly.Blazor.Layout? layout2, string title, string xTitle, string yTitle)
        {
            if (layout2 == null) return;
            layout2.Title = new Plotly.Blazor.LayoutLib.Title();
            layout2.Title.Text = title;
            layout2.Margin = new Plotly.Blazor.LayoutLib.Margin();
            layout2.Margin.L = new decimal(120);
            layout2.Margin.T = new decimal(30);
            layout2.Margin.R = new decimal(70);
            layout2.Margin.B = new decimal(30);
            layout2.YAxis = new List<YAxis>();
            layout2.YAxis.Add(new YAxis());
            layout2.YAxis[0].Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title();
            layout2.YAxis[0].Title.Text = yTitle;
            layout2.YAxis[0].ScaleAnchor = "x";
            layout2.YAxis[0].ScaleRatio = new decimal(1);
            layout2.YAxis[0].ExponentFormat = Plotly.Blazor.LayoutLib.YAxisLib.ExponentFormatEnum.e;
            layout2.YAxis[0].ShowExponent = Plotly.Blazor.LayoutLib.YAxisLib.ShowExponentEnum.All;
            
            layout2.XAxis = new List<XAxis>();
            layout2.XAxis.Add(new XAxis());
            string xTickFormat = "%H:%M:%S";
            layout2.XAxis[0].TickFormat = xTickFormat;
            layout2.XAxis[0].Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title();
            layout2.XAxis[0].Title.Text = xTitle;
            layout2.XAxis[0].LineWidth = new decimal(1);
            layout2.XAxis[0].Mirror = Plotly.Blazor.LayoutLib.XAxisLib.MirrorEnum.True;
            layout2.Height = new decimal(300);
            layout2.AutoSize = true;
        }


        protected override async Task OnInitializedAsync()
        {
            data_LS = new List<ITrace>();
            data_KF = new List<ITrace>();
            foreach(var name in PlotNames)
            {
                if (string.IsNullOrEmpty(name)) { continue; }
                //var scatLS = new Scatter
                var scatLS = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> {  },
                    Y = new List<object?> {  }
                };
                data_LS.Add(scatLS);

                //var scatKF = new Scatter
                var scatKF = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> {  },
                    Y = new List<object?> {  }
                };
                data_KF.Add(scatKF);
            }

            await Task.Run(() => GenerateLayout(layout_LS, "", "Time [GPS]", "Rec. Clock Error (LS) [sec]"));
            await Task.Run(() => GenerateLayout(layout_KF, "", "Time [GPS]", "Rec. Clock Error (KF) [sec]"));
            //await chart_EN.Relayout();
        }


        //protected override async Task OnAfterRenderAsync(bool firstRender)
        protected override async Task OnParametersSetAsync()
        {
            if (ActiveKey != RenderKey) return;
            if (chart_LS == null || chart_KF == null) return;

            if (Data1 != null && !HaveRendered)
            {
                for (int i = 0; i < MAX_SYS; i++)
                {
                    var x = new List<object>();
                    var y = new List<object>();
                    foreach (var d in Data1)
                    {
                        if (d == null || double.IsNaN(d.RecCLK[i])) continue;
                        x.Add(d.Time.ToString("O"));
                        y.Add(d.RecCLK[i]);
                    }
                    await chart_LS.ExtendTrace(x, y, i);
                }
            }
            if (Data2 != null && !HaveRendered)
            {
                for (int i = 0; i < MAX_SYS; i++)
                {
                    var x = new List<object>();
                    var y = new List<object>();
                    foreach (var d in Data2)
                    {
                        if (d == null || double.IsNaN(d.RecCLK[i])) continue;
                        x.Add(d.Time.ToString("O"));
                        y.Add(d.RecCLK[i]);
                    }
                    await chart_KF.ExtendTrace(x, y, i);
                }
            }
            chart_LS.Update();
            chart_KF.Update();
            if (FilePlot)
            {
                HaveRendered = true;
            }
        }
    }
}
