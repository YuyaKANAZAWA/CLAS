using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;

namespace GritZ3.Components.Plots
{
    public partial class ENUPlot
    {
        [Parameter]
        public string[]? PlotNames { get; set; }
        [Parameter]
        public List<EpochPosData?>? Data1 { get; set; }
        [Parameter]
        public List<EpochPosData?>? Data2 { get; set; }
        [Parameter]
        public List<EpochPosData?>? Data3 { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }
        [Parameter]
        public bool FilePlot { get; set; }

        private bool HaveRendered1 { get; set; } = false;   // fileデータプロット時の描画制御用
        private bool HaveRendered2 { get; set; } = false;
        private bool HaveRendered3 { get; set; } = false;

        PlotlyChart? chart_E;
        PlotlyChart? chart_N;
        PlotlyChart? chart_U;
        Config config_E = new Config() { Responsive = true };
        Config config_N = new Config() { Responsive = true };
        Config config_U = new Config() { Responsive = true };
        Plotly.Blazor.Layout layout_E = new();
        Plotly.Blazor.Layout layout_N = new();
        Plotly.Blazor.Layout layout_U = new();

        // Using of the interface IList is important for the event callback!
        IList<ITrace> data_E;
        IList<ITrace> data_N;
        IList<ITrace> data_U;

        private void GenerateLayout(Plotly.Blazor.Layout? layout2, string title, string xTitle, string yTitle)
        {
            if (layout2 == null) return;
            layout2.Title = new Plotly.Blazor.LayoutLib.Title();
            layout2.Title.Text = title;
            layout2.Margin = new Plotly.Blazor.LayoutLib.Margin();
            layout2.Margin.L = new decimal(70);
            layout2.Margin.T = new decimal(30);
            layout2.Margin.R = new decimal(70);
            layout2.Margin.B = new decimal(30);
            layout2.YAxis = new List<YAxis>();
            layout2.YAxis.Add(new YAxis());
            layout2.YAxis[0].Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title();
            layout2.YAxis[0].Title.Text = yTitle;
            layout2.YAxis[0].ScaleAnchor = "x";
            layout2.YAxis[0].ScaleRatio = new decimal(1);
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
            data_E = new List<ITrace>();
            data_N = new List<ITrace>();
            data_U = new List<ITrace>();
            foreach(var name in PlotNames)
            {
                if (string.IsNullOrEmpty(name)) { continue; }
                var scatE = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> {  },
                    Y = new List<object?> {  }
                };
                data_E.Add(scatE);

                var scatN = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> {  },
                    Y = new List<object?> {  }
                };
                data_N.Add(scatN);

                var scatU = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> {  },
                    Y = new List<object?> {  }
                };
                data_U.Add(scatU);
            }

            await Task.Run(() => GenerateLayout(layout_E, "", "Time [GPS]", "East [m]"));
            await Task.Run(() => GenerateLayout(layout_N, "", "Time [GPS]", "North [m]"));
            await Task.Run(() => GenerateLayout(layout_U, "", "Time [GPS]", "Up [m]"));
        }

        protected override async Task OnParametersSetAsync()
        //protected override void OnParametersSet()
        {
            if (ActiveKey != RenderKey) return;
            if (chart_E == null || chart_N == null || chart_U == null) return;
            if(Data1 != null && !HaveRendered1)
            {
                var x = new List<object>();
                var E = new List<object>();
                var N = new List<object>();
                var U = new List<object>();
                foreach(var d in Data1)
                {
                    if(d==null) continue;
                    if (!double.IsNaN(d.East) && !double.IsNaN(d.North) && !double.IsNaN(d.Up))
                    {
                        x.Add(d.Time.ToString("O"));
                        E.Add(d.East);
                        N.Add(d.North);
                        U.Add(d.Up);
                    }
                }
                await chart_E.ExtendTrace(x, E, 0);
                await chart_N.ExtendTrace(x, N, 0);
                await chart_U.ExtendTrace(x, U, 0);
                if(FilePlot)
                {
                    HaveRendered1 = true;
                }   
                //chart_E.ExtendTrace(x, E, 0);
                //chart_N.ExtendTrace(x, N, 0);
                //chart_U.ExtendTrace(x, U, 0);
            }
            if (Data2 != null && !HaveRendered2)
            {
                var x = new List<object>();
                var E = new List<object>();
                var N = new List<object>();
                var U = new List<object>();
                foreach (var d in Data2)
                {
                    if(d==null) continue;
                    if (!double.IsNaN(d.East) && !double.IsNaN(d.North) && !double.IsNaN(d.Up))
                    {
                        x.Add(d.Time.ToString("O"));
                        E.Add(d.East);
                        N.Add(d.North);
                        U.Add(d.Up);
                    }
                }
                await chart_E.ExtendTrace(x, E, 1);
                await chart_N.ExtendTrace(x, N, 1);
                await chart_U.ExtendTrace(x, U, 1);
                if (FilePlot)
                {
                    HaveRendered2 = true;
                }
                //chart_E.ExtendTrace(x, E, 1);
                //chart_N.ExtendTrace(x, N, 1);
                //chart_U.ExtendTrace(x, U, 1);
            }
            if (Data3 != null && !HaveRendered3)
            {
                var x = new List<object>();
                var E = new List<object>();
                var N = new List<object>();
                var U = new List<object>();
                foreach (var d in Data3)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.East) && !double.IsNaN(d.North) && !double.IsNaN(d.Up))
                    {
                        x.Add(d.Time.ToString("O"));
                        E.Add(d.East);
                        N.Add(d.North);
                        U.Add(d.Up);
                    }
                }
                await chart_E.ExtendTrace(x, E, 2);
                await chart_N.ExtendTrace(x, N, 2);
                await chart_U.ExtendTrace(x, U, 2);
                //chart_E.ExtendTrace(x, E, 2);
                //chart_N.ExtendTrace(x, N, 2);
                //chart_U.ExtendTrace(x, U, 2);
                if (FilePlot)
                {
                    HaveRendered3 = true;
                }

            }
            await chart_E.Update();
            await chart_N.Update();
            await chart_U.Update();
            //chart_E.Update();
            //chart_N.Update();
            //chart_U.Update();
        }
    }
}
