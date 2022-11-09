using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Output
{
    public class KmlOut
    {


        public string FilePath { get; set; } = string.Empty;

        public async Task WriteAsync(List<EpochPosData> epochPos)
        {
            if(FilePath == string.Empty)
            {
                return;
            }
            await Task.Run(() =>
            {
                var sb = new StringBuilder();

                sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sb.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
                sb.AppendLine("  <Document>");
                sb.AppendLine("    <name>Demo</name>");
                sb.AppendLine("    <description>Description Demo</description>");
                sb.AppendLine("    <Placemark>");
                sb.AppendLine("      <name>Track Title</name>");
                sb.AppendLine("      <description>Track Description</description>");
                sb.AppendLine("      <Style>");
                sb.AppendLine("        <LineStyle>");
                sb.AppendLine("          <color>FF1400BE</color>");
                sb.AppendLine("          <width>4</width>");
                sb.AppendLine("        </LineStyle>");
                sb.AppendLine("      </Style>");
                sb.AppendLine("      <LineString>");
                sb.AppendLine("        <extrude>1</extrude>");
                sb.AppendLine("        <tessellate>1</tessellate>");
                sb.AppendLine("        <altitudeMode>clampToGround</altitudeMode>");
                sb.AppendLine("        <coordinates>");
                foreach(var ep in epochPos)
                {
                    sb.AppendLine($"        {ep.Lon:F8},{ep.Lat:F8},{ep.Alt:F4}");
                }
                sb.AppendLine("        </coordinates>");
                sb.AppendLine("      </LineString>");
                sb.AppendLine("    </Placemark>");
                //
                sb.AppendLine("    <Folder>");
                sb.AppendLine("      <name>Rover Position</name>");
                foreach(var ep in epochPos)
                {
                    string timefordisp = $"{ep.Time.ToString("yyyy")}-{ep.Time.ToString("MM")}-{ep.Time.ToString("dd")}T{ep.Time.ToString("HH")}:{ep.Time.ToString("mm")}:{ep.Time.ToString("ss")}.{ep.Time.ToString("ff")}Z";
                    sb.AppendLine("      <Placemark>");
                    sb.AppendLine($"        <TimeStamp><when>{timefordisp}</when></TimeStamp>");
                    sb.AppendLine($"        <name>{timefordisp}</name>");
                    sb.AppendLine($"        <description>AAA</description>");
                    sb.AppendLine("        <Style>");
                    sb.AppendLine("          <IconStyle>");
                    sb.AppendLine("            <scale>0.2</scale>");
                    sb.AppendLine("            <color>ffff0000</color>");
                    sb.AppendLine("            <Icon><href>http://maps.google.com/mapfiles/kml/pal2/icon26.png</href></Icon>");
                    sb.AppendLine("          </IconStyle>");
                    sb.AppendLine("        </Style>");
                    sb.AppendLine("        <Point>");
                    sb.AppendLine($"          <coordinates>{ep.Lon:F8},{ep.Lat:F8},{ep.Alt:F4}</coordinates>");
                    sb.AppendLine("        </Point>");
                    sb.AppendLine("      </Placemark>");
                }
                sb.AppendLine("    </Folder>");
                //
                sb.AppendLine("  </Document>");
                sb.AppendLine("</kml>");
                File.WriteAllText(FilePath, sb.ToString());
            });
        }





    }
}


