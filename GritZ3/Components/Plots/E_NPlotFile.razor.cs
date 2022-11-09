using GritZ3.Classes;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;

namespace GritZ3.Components.Plots
{
    public partial class E_NPlotFile
    {
        [Parameter]
        public DataForPlot? dataForPlot { get; set; }
        //[Parameter]
        //public bool E_NRendered { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }

        private bool haveRendered = false;

        PlotlyChart? chart_EN;
        Config config = new Config() { Responsive = true };
        Plotly.Blazor.Layout layout_EN = new();

        // Using of the interface IList is important for the event callback!
        IList<ITrace>? data_EN;
        //IList<ITrace> data_EN = new List<ITrace>
        //{
        //    new ScatterGl
        //    {
        //        Name = $"ScatterTrace",
        //        Mode = ModeFlag.Lines | ModeFlag.Markers,
        //        X = new List<object>{},
        //        Y = new List<object>{}
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
            await Task.Run(() => GenerateLayout(layout_EN, "", "East [m]", "North [m]"));

            //if (chart_EN == null) return;
            //await Task.Run(() => GenerateLayout(layout_EN, "", "East [m]", "North [m]"));
            //await chart_EN.Relayout();
        }


        // OnParametersSetAsyncは内部の処理の度にレンダリングされ，全部終了後に再度レンダリングされる
        // そのまま処理すると，タブ切り換え時に2回描画され遅く，バタバタになるので，bool UpdatePlotで呼び出し側から制御するようにした
        protected override async Task OnParametersSetAsync()
        {
            if (dataForPlot == null || ActiveKey != RenderKey || chart_EN == null) return;
            //await chart_EN.Clear();

            if (!haveRendered)
            {
                await chart_EN.Clear();
                await chart_EN.AddTrace(
                    new ScatterGl
                    {
                        Name = $"ScatterTrace",
                        Mode = ModeFlag.Lines | ModeFlag.Markers,
                        X = dataForPlot.East.Cast<object>().ToList(),
                        Y = dataForPlot.North.Cast<object>().ToList()
                    });

                //await chart_EN.Relayout();

                haveRendered = true;
            }




        }




    }
}
