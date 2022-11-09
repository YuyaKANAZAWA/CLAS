using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;
using static GritZ3.Components.Plots.PlotDefinitions;

namespace GritZ3.Components.Plots
{
    public partial class E_NPlot
    {
        [Parameter]
        public string[]? PlotNames { get; set; }
        [Parameter]
        public List<EpochPosData>? Data1 { get; set; }
        [Parameter]
        public List<EpochPosData>? Data2 { get; set; }
        [Parameter]
        public List<EpochPosData>? Data3 { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }
        [Parameter]
        public bool FilePlot { get; set; }
        [Parameter]
        public PlotControl PlotCtrl { get; set; } = PlotControl.NoAction;

        private bool HaveRendered1 { get; set; } = false;   // fileデータプロット時の描画制御用
        private bool HaveRendered2 { get; set; } = false;
        private bool HaveRendered3 { get; set; } = false;


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
            foreach (var name in PlotNames)
            {
                if (string.IsNullOrEmpty(name)) { continue; }
                var scat = new ScatterGl
                {
                    Name = name,
                    Mode = ModeFlag.Lines | ModeFlag.Markers,
                    X = new List<object?> { },
                    Y = new List<object?> { }
                };
                data_EN.Add(scat);
            }
            await Task.Run(() => GenerateLayout(layout_EN, "", "East [m]", "North [m]"));
        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        protected override async Task OnParametersSetAsync()
        {
            if (ActiveKey != RenderKey || chart_EN == null) return;
            if (Data1 != null && !HaveRendered1)
            {
                var x = new List<object>();
                var y = new List<object>();
                foreach (var d in Data1)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.East) && !double.IsNaN(d.North))
                    {
                        x.Add(d.East);
                        y.Add(d.North);
                    }
                }
                await chart_EN.ExtendTrace(x, y, 0);
                if (FilePlot)                           // fileデータプロット時は1回描画したら再描画しないようにする
                {
                    HaveRendered1 = true;
                }
            }

            //if (ClearFlag)
            //{
            //   ClearFlag = false;

            //    await chart_EN.DeleteTrace(0);

            //    var x = new List<object>();
            //    var y = new List<object>();
            //    await Task.Run(() =>
            //    {
            //        foreach (var d in Data1)
            //        {
            //            if (d == null) continue;
            //            if (!double.IsNaN(d.East) && !double.IsNaN(d.North))
            //            {
            //                x.Add(d.East);
            //                y.Add(d.North);
            //            }
            //        }

            //    });

            //    await chart_EN.AddTrace(new ScatterGl
            //    {
            //        Name = $"ScatterTrace",
            //        Mode = ModeFlag.Lines | ModeFlag.Markers,
            //        X = x,
            //        Y = y
            //    });


            //}

            if (Data2 != null && !HaveRendered2)
            {
                var x = new List<object>();
                var y = new List<object>();
                foreach (var d in Data2)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.East) && !double.IsNaN(d.North))
                    {
                        x.Add(d.East);
                        y.Add(d.North);
                    }
                }
                await chart_EN.ExtendTrace(x, y, 1);
                if (FilePlot)                           // fileデータプロット時は1回描画したら再描画しないようにする
                {
                    HaveRendered2 = true;
                }
            }
            if (Data3 != null && !HaveRendered3)
            {
                var x = new List<object>();
                var y = new List<object>();
                foreach (var d in Data3)
                {
                    if (d == null) continue;
                    if (!double.IsNaN(d.East) && !double.IsNaN(d.North))
                    {
                        x.Add(d.East);
                        y.Add(d.North);
                    }
                }
                await chart_EN.ExtendTrace(x, y, 2);
                if (FilePlot)                           // fileデータプロット時は1回描画したら再描画しないようにする
                {
                    HaveRendered3 = true;
                }
            }
            //await chart_EN.Update();
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (PlotCtrl == PlotControl.ReDraw || PlotCtrl == PlotControl.Clear)
            {
                await chart_EN.DeleteTrace(0);

                if (PlotCtrl == PlotControl.ReDraw)
                {

                    await InvokeAsync(() => { PlotCtrl = PlotControl.NoAction; });

                    var x = new List<object>();
                    var y = new List<object>();
                    await Task.Run(() =>
                    {
                        foreach (var d in Data1)
                        {
                            if (d == null) continue;
                            if (!double.IsNaN(d.East) && !double.IsNaN(d.North))
                            {
                                x.Add(d.East);
                                y.Add(d.North);
                            }
                        }

                    });
                    await chart_EN.AddTrace(new ScatterGl
                    {
                        Name = $"ScatterTrace",
                        Mode = ModeFlag.Lines | ModeFlag.Markers,
                        X = x,
                        Y = y
                    });
                    
                }
            }
            else if (PlotCtrl == PlotControl.Playing)
            {
                var x = new List<object>();
                var y = new List<object>();
                await Task.Run(() =>
                {
                    foreach (var d in Data1)
                    {
                        if (d == null) continue;
                        if (!double.IsNaN(d.East) && !double.IsNaN(d.North))
                        {
                            x.Add(d.East);
                            y.Add(d.North);
                        }
                    }

                });
                await chart_EN.ExtendTrace(x, y, 0);
            }
            //await InvokeAsync(()=> { PlotCtrl = PlotControl.NoAction;  });






        }
    }
}
