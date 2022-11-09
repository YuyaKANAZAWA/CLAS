using GritZ3.Classes;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;

namespace GritZ3.Components.Plots
{
    public partial class LLHPlotFile
    {
        [Parameter]
        public DataForPlot? dataForPlot { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }

        private bool haveRendered = false;

        PlotlyChart? chart_Lat;
        PlotlyChart? chart_Lon;
        PlotlyChart? chart_Alt;
        Config config = new Config() { Responsive = true };
        Plotly.Blazor.Layout layout_Lat = new();
        Plotly.Blazor.Layout layout_Lon = new();
        Plotly.Blazor.Layout layout_Alt = new();

        // Using of the interface IList is important for the event callback!
        IList<ITrace>? data_Lat;
        IList<ITrace>? data_Lon;
        IList<ITrace>? data_Alt;
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
            layout2.Margin.B = new decimal(40);
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
            layout2.Height = new decimal(300);
            layout2.AutoSize = true;

        }

        protected override async Task OnInitializedAsync()
        {
            data_Lat = new List<ITrace>();
            data_Lon = new List<ITrace>();
            data_Alt = new List<ITrace>();
            await Task.Run(() => GenerateLayout(layout_Lat, "", "Time (UTC)", "Latitude [deg]"));
            await Task.Run(() => GenerateLayout(layout_Lon, "", "Time (UTC)", "Latitude [deg]"));
            await Task.Run(() => GenerateLayout(layout_Alt, "", "Time (UTC)", "Altitude [m]"));
        }

        protected override async Task OnParametersSetAsync()
        {
            if (dataForPlot == null || ActiveKey != RenderKey 
                || chart_Lat == null || chart_Lon == null || chart_Alt == null) return;

            if (!haveRendered)
            {
                await chart_Lat.Clear();
                await chart_Lat.AddTrace(
                    new ScatterGl
                    {
                        Name = $"ScatterTrace",
                        Mode = ModeFlag.Lines | ModeFlag.Markers,
                        X = dataForPlot.TimeStr.Cast<object>().ToList(),
                        Y = dataForPlot.Lat.Cast<object>().ToList()
                    });

                await chart_Lon.Clear();
                await chart_Lon.AddTrace(
                    new ScatterGl
                    {
                        Name = $"ScatterTrace",
                        Mode = ModeFlag.Lines | ModeFlag.Markers,
                        X = dataForPlot.TimeStr.Cast<object>().ToList(),
                        Y = dataForPlot.Lon.Cast<object>().ToList()
                    });

                await chart_Alt.Clear();
                await chart_Alt.AddTrace(
                    new ScatterGl
                    {
                        Name = $"ScatterTrace",
                        Mode = ModeFlag.Lines | ModeFlag.Markers,
                        X = dataForPlot.TimeStr.Cast<object>().ToList(),
                        Y = dataForPlot.Alt.Cast<object>().ToList()
                    });

                haveRendered = true;
            }
        }
    }
}
