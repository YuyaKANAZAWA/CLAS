using GrToolBox.Output;
using LeafletOnBlazor;
using Microsoft.AspNetCore.Components;

namespace GritZ3.Components.Plots
{
    public partial class MapPlot
    {
        [Parameter]
        public List<EpochPosData> Data { get; set; }
        [Parameter]
        public string? ActiveKey { get; set; }
        [Parameter]
        public string? RenderKey { get; set; }
        [Parameter]
        public bool FilePlot { get; set; } = true;

        [Inject] public LayerFactory? LayerFactory { get; init; }

        private bool initRendered = false;
        private MapOptions? mapOptions;
        private Map? mapRef;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            mapOptions = new MapOptions()
            {
                PreferCanvas = true,
                Center = new LatLng(35.0, 135.0),
                Zoom = 12,
            };
        }

        protected override async Task OnParametersSetAsync()
        {
            if (mapRef == null || Data == null || !initRendered || FilePlot) return;
            int _zoom = (int)await mapRef.GetZoom();

            var circleMarkerOptions = new CircleMarkerOptions() { Radius = 3 };
            await InvokeAsync(() =>
            {
                if (LayerFactory != null)
                {
                    foreach (var d in Data)
                    {
                        if(d == null) continue;
                        var tmp = LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(d.Lat, d.Lon), mapRef, circleMarkerOptions);
                    }
                }
            });

            var d = Data.Last();
            if ((d != null) && !double.IsNaN(d.Lat) && !double.IsNaN(d.Lon))   // リアルタイム描画の時は，最新の点を中心にする
            {
                await InvokeAsync(() => mapRef.SetView(new LatLng(d.Lat, d.Lon), _zoom));
            }
        }


        private async Task AfterMapRender()
        {
            if (mapRef == null || LayerFactory == null) return;
            //Create Tile Layer
            var tileLayerOptions = new TileLayerOptions()
            {
                Attribution = "&copy; <a href=\"https://www.openstreetmap.org/copyright\">OpenStreetMap</a> contributors"
            };
            var mainTileLayer = await LayerFactory.CreateTileLayerAndAddToMap("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", mapRef, tileLayerOptions);
            initRendered = true;

            // Fileの時（データが揃っているとき）はこのタイミングで描画すれば良い
            if (FilePlot)   // fileの時は，スタート地点を中心にして描画遅いのをごまかす
            {
                if(Data != null)
                {
                    int _zoom = (int)await mapRef.GetZoom();
                    var d = Data.First();
                    if (!double.IsNaN(d.Lat) && !double.IsNaN(d.Lon))
                    {
                        await mapRef.SetView(new LatLng(d.Lat, d.Lon), _zoom);
                    }
                    var circleMarkerOptions = new CircleMarkerOptions() { Radius = 3 };
                    foreach (var data in Data)
                    {
                        var tmp = LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(data.Lat, data.Lon), mapRef, circleMarkerOptions);
                    }
                }
            }

            //if (Data != null)
            //{
            //    var centLat = Data.First().Lat;
            //    var centLon = Data.First().Lon;

            //    await mapRef.SetView(new LatLng(centLat, centLon), 12);


            //    var circleMarkerOptions = new CircleMarkerOptions() { Radius = 3 };

            //    for (int i = 0; i < dataForPlot.TimeStr.Count(); i++)
            //    {
            //        var circleMarker = LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(dataForPlot.Lat[i], dataForPlot.Lon[i]), mapRef, circleMarkerOptions);
            //    }

            //}
        }


    }
}
