using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GrToolBox.Settings
{
    public class SettingsUtilities
    {
        public static void SaveSettingsJson(string fname, GrSettings stg)
        {
            var opt = new JsonSerializerOptions();
            opt.WriteIndented = true;
            opt.Converters.Add(new JsonStringEnumConverter());
            string json = JsonSerializer.Serialize(stg, opt);
            //Console.WriteLine(json);
            File.WriteAllText(@fname, json);
        }

        public static GrSettings LoadSettingsJson(string fname)
        {
            var opt = new JsonSerializerOptions();
            opt.Converters.Add(new JsonStringEnumConverter());
            opt.ReadCommentHandling = JsonCommentHandling.Skip;
            var loaded_txt = File.ReadAllText(@fname);
            var loaded_stg = JsonSerializer.Deserialize<GrSettings>(loaded_txt, opt);
            return loaded_stg;
        }
    }



    


}
