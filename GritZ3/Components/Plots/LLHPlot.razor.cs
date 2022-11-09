using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;

namespace GritZ3.Components.Plots
{
    public partial class LLHPlot
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
            //layout2.Title = new Plotly.Blazor.LayoutLib.Title();
            //layout2.Title.Text = title;
            //layout2.Margin = new Plotly.Blazor.LayoutLib.Margin();
            //layout2.Margin.L = new decimal(70);
            //layout2.Margin.T = new decimal(30);
            //layout2.Margin.R = new decimal(70);
            //layout2.Margin.B = new decimal(40);
            //layout2.YAxis = new List<YAxis>();
            //layout2.YAxis.Add(new YAxis());
            //layout2.YAxis[0].Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title();
            //layout2.YAxis[0].Title.Text = yTitle;
            //layout2.YAxis[0].ScaleAnchor = "x";
            //layout2.YAxis[0].ScaleRatio = new decimal(1);
            //layout2.XAxis = new List<XAxis>();
            //layout2.XAxis.Add(new XAxis());
            //layout2.XAxis[0].Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title();
            //layout2.XAxis[0].Title.Text = xTitle;
            //layout2.XAxis[0].LineWidth = new decimal(1);
            //layout2.XAxis[0].Mirror = Plotly.Blazor.LayoutLib.XAxisLib.MirrorEnum.True;
            //layout2.Height = new decimal(300);
            //layout2.AutoSize = true;
            layout2.Title = new Plotly.Blazor.LayoutLib.Title();
            layout2.Title.Text = title;
            layout2.Margin = new Plotly.Blazor.LayoutLib.Margin();
            layout2.Margin.L = new decimal(100);
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
            data_Lat = new List<ITrace>();
            data_Lon = new List<ITrace>();
            data_Alt = new List<ITrace>();

            foreach (var name in PlotNames)
            {
                if (string.IsNullOrEmpty(name)) { continue; }
                var scatLat = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> { },
                    Y = new List<object?> { }
                };
                data_Lat.Add(scatLat);

                var scatLon = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> { },
                    Y = new List<object?> { }
                };
                data_Lon.Add(scatLon);

                var scatAlt = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> { },
                    Y = new List<object?> { }
                };
                data_Alt.Add(scatAlt);
            }
            await Task.Run(() => GenerateLayout(layout_Lat, "", "Time (GPS)", "Latitude [deg]"));
            await Task.Run(() => GenerateLayout(layout_Lon, "", "Time (GPS", "Longitude [deg]"));
            await Task.Run(() => GenerateLayout(layout_Alt, "", "Time (GPS)", "Altitude [m]"));
        }

        protected override async Task OnParametersSetAsync()
        {
            if (ActiveKey != RenderKey) return;
            if (chart_Lat == null || chart_Lon == null || chart_Alt == null) return;

            if (Data1 != null && !HaveRendered1)
            {
                var x = new List<object>();
                var lat = new List<object>();
                var lon = new List<object>();
                var alt = new List<object>();
                foreach (var d in Data1)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.Lat) && !double.IsNaN(d.Lon) && !double.IsNaN(d.Alt))
                    {
                        x.Add(d.Time.ToString("O"));
                        lat.Add(d.Lat);
                        lon.Add(d.Lon);
                        alt.Add(d.Alt);
                    }
                }
                await chart_Lat.ExtendTrace(x, lat, 0);
                await chart_Lon.ExtendTrace(x, lon, 0);
                await chart_Alt.ExtendTrace(x, alt, 0);
                if (FilePlot)
                {
                    HaveRendered1 = true;
                }
            }

            if (Data2 != null && !HaveRendered2)
            {
                var x = new List<object>();
                var lat = new List<object>();
                var lon = new List<object>();
                var alt = new List<object>();
                foreach (var d in Data2)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.Lat) && !double.IsNaN(d.Lon) && !double.IsNaN(d.Alt))
                    {
                        x.Add(d.Time.ToString("O"));
                        lat.Add(d.Lat);
                        lon.Add(d.Lon);
                        alt.Add(d.Alt);
                    }
                }
                await chart_Lat.ExtendTrace(x, lat, 1);
                await chart_Lon.ExtendTrace(x, lon, 1);
                await chart_Alt.ExtendTrace(x, alt, 1);
                if (FilePlot)
                {
                    HaveRendered2 = true;
                }
            }

            if (Data3 != null && !HaveRendered3)
            {
                var x = new List<object>();
                var lat = new List<object>();
                var lon = new List<object>();
                var alt = new List<object>();
                foreach (var d in Data3)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.Lat) && !double.IsNaN(d.Lon) && !double.IsNaN(d.Alt))
                    {
                        x.Add(d.Time.ToString("O"));
                        lat.Add(d.Lat);
                        lon.Add(d.Lon);
                        alt.Add(d.Alt);
                    }
                }
                await chart_Lat.ExtendTrace(x, lat, 2);
                await chart_Lon.ExtendTrace(x, lon, 2);
                await chart_Alt.ExtendTrace(x, alt, 2);
                if (FilePlot)
                {
                    HaveRendered3 = true;
                }
            }
            await chart_Lat.Update();
            await chart_Lon.Update();
            await chart_Alt.Update();
        }
    }
}
