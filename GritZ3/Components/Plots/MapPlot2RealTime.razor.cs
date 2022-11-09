using GritZ3.Classes;
using GrToolBox.Output;
using LeafletOnBlazor;
using Microsoft.AspNetCore.Components;

namespace GritZ3.Components.Plots
{
    public partial class MapPlot2RealTime
    {
        [Parameter]
        public EpochPosData? EpochPosData { get; set; }

        [Inject] public LayerFactory? LayerFactory { get; init; }

        private MapOptions? mapOptions;
        private Map? mapRef;

        private bool initRendered = false;

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

        protected override async Task OnParametersSetAsync()
        {

            if (mapRef == null || EpochPosData == null || !initRendered) return;
            int _zoom = (int)await mapRef.GetZoom();
            var circleMarkerOptions = new CircleMarkerOptions() { Radius = 3 };
            await InvokeAsync(() => LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(EpochPosData.Lat, EpochPosData.Lon), mapRef, circleMarkerOptions));
            //await InvokeAsync(() => mapRef.SetView(new LatLng(EpochPosData.Lat, EpochPosData.Lon), 12));
            await InvokeAsync(() => mapRef.SetView(new LatLng(EpochPosData.Lat, EpochPosData.Lon), _zoom));


            //    await mapRef.SetView(new LatLng(aveLat, aveLon), 12);

            //var circleMarker = await LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(EpochPosData.Lat, EpochPosData.Lon), mapRef, circleMarkerOptions);

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

//            InvokeAsync(() => mapRef.SetView(new LatLng(35.0, 135.0), 12));



            //mapRef.SetView(new LatLng(35.0, 135.0), 12);


            initRendered = true;


            //if (EpochPosData != null)
            //{
            //    var aveLat = EpochPosData.Lat;
            //    var aveLon = EpochPosData.Lon;
            //    await mapRef.SetView(new LatLng(aveLat, aveLon), 12);
            //    //var circleMarkerOptions = new CircleMarkerOptions() { Radius = 3 };
            //    //var circleMarker = LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(EpochPosData.Lat, EpochPosData.Lon), mapRef, circleMarkerOptions);
            //    //for (int i = 0; i < dataForPlot.TimeStr.Count(); i++)
            //    //{
            //    //    var circleMarker = LayerFactory.CreateCircleMarkerAndAddToMap(new LatLng(dataForPlot.Lat[i], dataForPlot.Lon[i]), mapRef, circleMarkerOptions);
            //    //}
            //}
        }


    }
}
