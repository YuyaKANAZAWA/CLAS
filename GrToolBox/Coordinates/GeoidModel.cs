using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static GrToolBox.GrMath.Interpolation;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Coordinates
{
    public class GeoidModel
    {
        int lat_min, lat_max, lon_min, lon_max;
        double dlon, dlat;
        double[][] EGM96_025grid;


        /*----------------
            Constructor
        -----------------*/
        public GeoidModel(Geoid_Type geoid_Type)
        {
            switch (geoid_Type)
            {
                case Geoid_Type.EGM96:
                    ReadEGM96_025();

                    break;
            }
        }

        /*----------------
            public methods
        -----------------*/
        public double GetGeoidHeight(double lat_deg, double lon_deg)
        {
            int nRow = EGM96_025grid.Length;
            int nCol = EGM96_025grid[0].Length;
            return Interp2(lon_min, lon_max, dlon, lat_min, lat_max, dlat, EGM96_025grid, lon_deg, lat_deg);
        }

        /*----------------
            private methods
        -----------------*/
        private void ReadEGM96_025()
        {
            //string fn = ".\\Coordinates\\GeoidData\\EGM96_025.txt";
            //string fn = System.Environment.CurrentDirectory + @"\Coordinates\GeoidData\EGM96_025.txt";
            string fn = System.Environment.CurrentDirectory + @"/Coordinates/GeoidData/EGM96_025.txt";
            string fn_path = System.IO.Path.GetFullPath(fn);
            var Raw = File.ReadAllLines(fn_path, Encoding.UTF8);
            //var Raw = File.ReadAllLines(fn, Encoding.UTF8);

            lat_min = -90;
            lat_max = 90;
            lon_min = 0;
            lon_max = 360;
            dlon = 0.25;
            dlat = 0.25;
            int nRow = 721;
            int nCol = 1441;

            EGM96_025grid = new double[nRow][];

            for(int i=0; i<nRow; i++)
            {
                string[] tmp = Raw[i].Split(',');
                EGM96_025grid[i] = new double[nCol];
                for(int j=0; j<nCol; j++)
                {
                    EGM96_025grid[i][j] = double.Parse(tmp[j].Trim());
                }
            }
        }



    }


}

