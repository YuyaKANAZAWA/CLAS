using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
//using Plotly.Blazor.Traces.ScatterLib;
using Plotly.Blazor.Traces.ScatterGlLib;

namespace GritZ3.Components.Plots
{
    public partial class ENUPlotRealTime2
    {
        [Parameter]
        public string[]? PlotNames { get; set; }
        [Parameter]
        public EpochPosData? Data1 { get; set; }
        [Parameter]
        public EpochPosData? Data2 { get; set; }
        [Parameter]
        public EpochPosData? Data3 { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }

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

        //IList<ITrace> data_EN = new List<ITrace>
        //{
        //    new Scatter
        //    {
        //        Name = $"Gr-LS",
        //        Mode = ModeFlag.Lines | ModeFlag.Markers,
        //        X = new List<object?>{0},
        //        Y = new List<object?>{0}
        //    },
        //    new Scatter
        //    {
        //        Name = $"Gr-KF",
        //        Mode = ModeFlag.Lines | ModeFlag.Markers,
        //        X = new List<object?>{0},
        //        Y = new List<object?>{0}
        //    },
        //    new Scatter
        //    {
        //        Name = $"Receiver",
        //        Mode = ModeFlag.Lines | ModeFlag.Markers,
        //        X = new List<object?>{0},
        //        Y = new List<object?>{0}
        //    }
        //};


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
            //await chart_EN.Relayout();
        }

        //protected override async Task OnParametersSetAsync()
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (ActiveKey != RenderKey) return;
            if (chart_E == null || chart_N == null || chart_U == null) return;
            if(Data1 != null)
            {
                if (!double.IsNaN(Data1.East) && !double.IsNaN(Data1.North) && !double.IsNaN(Data1.Up))
                {
                    var x = new List<object> { Data1.Time.ToString("O") };
                    var E = new List<object> { Data1.East };
                    var N = new List<object> { Data1.North };
                    var U = new List<object> { Data1.Up };
                    chart_E.ExtendTrace(x, E, 0);
                    chart_N.ExtendTrace(x, N, 0);
                    chart_U.ExtendTrace(x, U, 0);
                }
            }
            if(Data2 != null)
            {
                if (!double.IsNaN(Data2.East) && !double.IsNaN(Data2.North) && !double.IsNaN(Data2.Up))
                {
                    var x = new List<object> { Data2.Time.ToString("O") };
                    var E = new List<object> { Data2.East };
                    var N = new List<object> { Data2.North };
                    var U = new List<object> { Data2.Up };
                    chart_E.ExtendTrace(x, E, 1);
                    chart_N.ExtendTrace(x, N, 1);
                    chart_U.ExtendTrace(x, U, 1);
                }
            }
            if (Data3 != null)
            {
                if (!double.IsNaN(Data3.East) && !double.IsNaN(Data3.North) && !double.IsNaN(Data3.Up))
                {
                    var x = new List<object> { Data3.Time.ToString("O") };
                    var E = new List<object> { Data3.East };
                    var N = new List<object> { Data3.North };
                    var U = new List<object> { Data3.Up };
                    chart_E.ExtendTrace(x, E, 2);
                    chart_N.ExtendTrace(x, N, 2);
                    chart_U.ExtendTrace(x, U, 2);
                }
            }

            //await Task.Run(() =>
            //{
            //    chart_E.Update();
            //    chart_N.Update();
            //    chart_U.Update();
            //});
            chart_E.Update();
            chart_N.Update();
            chart_U.Update();


            //if (ActiveKey == "3")
            //{
            //     chart_E.Update();
            //     chart_N.Update();
            //     chart_U.Update();
            //    //await chart_EN.Relayout();
            //}
        }
    }
}
