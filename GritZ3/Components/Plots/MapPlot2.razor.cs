using GritZ3.Classes;
using LeafletOnBlazor;
using Microsoft.AspNetCore.Components;

namespace GritZ3.Components.Plots
{
    public partial class MapPlot2
    {
        [Parameter]
        public DataForPlot? dataForPlot { get; set; }
        [Parameter]
        public bool MapRendered { get; set; }

        [Inject] public LayerFactory? LayerFactory { get; init; }

        private MapOptions? mapOptions;
        private Map? mapRef;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            mapOptions = new MapOptions()
            {
                PreferCanvas = true,
                Center = new LatLng(35.0, 135.0),
                Zoom = 15,
            };
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

            if (dataForPlot != null)
            {
                //var aveLat = dataForPlot.Lat.Average();
                //var aveLon = dataForPlot.Lon.Average();
                var aveLat = dataForPlot.Lat.First();
                var aveLon = dataForPlot.Lon.First();

                await mapRef.SetView(new LatLng(aveLat, aveLon), 12);


                var circleMarkerOptions = new CircleMarkerOptions() { Radius = 3 };

                for (int i = 0; i < dataForPlot.TimeStr.Count(); i++)
                {
                    var circleMarker = LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(dataForPlot.Lat[i], dataForPlot.Lon[i]), mapRef, circleMarkerOptions);
                }

            }
        }


    }
}
