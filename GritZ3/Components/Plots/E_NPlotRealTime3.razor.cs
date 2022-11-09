using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;


namespace GritZ3.Components.Plots
{
    public partial class E_NPlotRealTime3
    {
        [Parameter]
        public string[]? PlotNames { get; set; }
        [Parameter]
        public List<EpochPosData?> Data { get; set; }
        //[Parameter]
        //public EpochPosData? Data2 { get; set; }
        //[Parameter]
        //public EpochPosData? Data3 { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string RenderKey { get; set; }


        PlotlyChart? chart_EN;
        Config config = new Config() { Responsive = true };
        Plotly.Blazor.Layout layout_EN = new();

        // Using of the interface IList is important for the event callback!
        IList<ITrace>? data_EN;
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
            layout2.Margin.B = new decimal(170);
            layout2.YAxis = new List<YAxis>();
            layout2.YAxis.Add(new YAxis());
            layout2.YAxis[0].Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title();
            layout2.YAxis[0].Title.Text = yTitle;
            layout2.YAxis[0].ScaleAnchor = "x";
            layout2.YAxis[0].ScaleRatio = new decimal(1);
            layout2.XAxis = new List<XAxis>();
            layout2.XAxis.Add(new XAxis());
            layout2.XAxis[0].Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title();
            layout2.XAxis[0].Title.Text = xTitle;
            layout2.XAxis[0].LineWidth = new decimal(1);
            layout2.XAxis[0].Mirror = Plotly.Blazor.LayoutLib.XAxisLib.MirrorEnum.True;
            layout2.Height = new decimal(700);
            layout2.AutoSize = true;
        }


        protected override async Task OnInitializedAsync()
        {
            data_EN = new List<ITrace>();
            foreach(var name in PlotNames)
            {
                if (string.IsNullOrEmpty(name)) { continue; }
                var scat = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> {  },
                    Y = new List<object?> {  }
                };
                data_EN.Add(scat);
            }
            await Task.Run(() => GenerateLayout(layout_EN, "", "East [m]", "North [m]"));
        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        protected override async Task OnParametersSetAsync()
        {
            if (ActiveKey != RenderKey || Data == null) return;
            var Data1 = Data[0];
            if (Data1 != null)
            {
                if (!double.IsNaN(Data1.East) && !double.IsNaN(Data1.North))
                {
                    var x = new List<object> { Data1.East };
                    var y = new List<object> { Data1.North };
                    chart_EN.ExtendTrace(x, y, 0);
                }
            }
            var Data2 = Data[1];
            if (Data2 != null)
            {
                if (!double.IsNaN(Data2.East) && !double.IsNaN(Data2.North))
                {
                    var x = new List<object> { Data2.East };
                    var y = new List<object> { Data2.North };
                    chart_EN.ExtendTrace(x, y, 1);
                }
            }
            var Data3 = Data[2];
            if (Data3 != null)
            {
                if (!double.IsNaN(Data3.East) && !double.IsNaN(Data3.North))
                {
                    var x = new List<object> { Data3.East };
                    var y = new List<object> { Data3.North };
                    chart_EN.ExtendTrace(x, y, 2);
                }
            }
            chart_EN.Update();
        }
    }
}
