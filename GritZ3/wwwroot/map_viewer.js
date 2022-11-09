let ePos, nPos, uPos, lat, lon, alt;
let lat_no_NaN = [], lon_no_NaN = [], time_no_NaN = [];

function plot_Map2() {
    console.log("entered");
    //document.getElementById("my_MainPlotArea").innerHTML = "";
    let map = L.map("my_wrapper").setView([35.0, 135.0, 15]);
    console.log(L);
    let gsi = L.tileLayer("https://cyberjapandata.gsi.go.jp/xyz/std/{z}/{x}/{y}.png", { attribution: "<a href='https://maps.gsi.go.jp/development/ichiran.html' target='_blank'>地理院タイル</a>" });
    let osm = L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { attribution: "© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors" });
    let baseMaps = { "GSI map": gsi, "OpenStreetMap": osm };
    L.control.layers(baseMaps).addTo(map);
    gsi.addTo(map);
}


function plot_Map() {
    //console.log(lat);
    document.getElementById("MainPlotArea").innerHTML = "";
    map = L.map('MainPlotArea').setView([lat_no_NaN[0], lon_no_NaN[0]], 15);
    let gsi = L.tileLayer('https://cyberjapandata.gsi.go.jp/xyz/std/{z}/{x}/{y}.png', { attribution: "<a href='https://maps.gsi.go.jp/development/ichiran.html' target='_blank'>地理院タイル</a>" });
    let osm = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: "© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors" });
    let baseMaps = { "GSI map": gsi, "OpenStreetMap": osm };
    L.control.layers(baseMaps).addTo(map);
    gsi.addTo(map);


    let myRenderer = L.canvas({ padding: 0.5 });

    for (let i = 0; i < lat.length; i++) {
        //if ((lat[i] === "NaN") || (lon[i] === "NaN")) {
        //    continue;
        //}
        //console.log(lat[i]);
        L.circleMarker([lat_no_NaN[i], lon_no_NaN[i]], { renderer: myRenderer, radius: 3 }).addTo(map).bindPopup(time_no_NaN[i]);
    }
}



function load_data() {
    //console.log(pos);
    ({ ePos, nPos, uPos, lat, lon, alt } = pos);

    for (let i = 0; i < lat.length; i++) {
        if ((lat[i] === "NaN") || (lon[i] === "NaN")) {
            continue;
        }
        lat_no_NaN.push(lat[i]);
        lon_no_NaN.push(lon[i]);
        time_no_NaN.push(time[i]);
    }
    plot_Map();
}








