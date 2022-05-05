using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;


namespace Kaidozu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\DEM.tif";
            string abovesavefilepath = @"D:\地上.tif";
            string belowsavefilepath = @"D:\地下.tif";

            SetDllDirectory(dllpath);

            // GeoTiff読み込み
            GdalRasterProvider d = new GdalRasterProvider();
            IRaster src = d.Open(loadfilepath);

            int ncol = src.NumColumns;
            int nrow = src.NumRows;
            int band_num = src.NumBands;
            string prj = src.ProjectionString;
            double nodata = src.NoDataValue;

            double[] pGT = src.Bounds.AffineCoefficients;
            double xllcenter = pGT[0];
            double cellsize_x = pGT[1];
            double rotate1 = pGT[2];
            double yllcenter = pGT[3];
            double rotate2 = pGT[4];
            double cellsize_y = pGT[5];

            // GeoTiff書き出し先作成
            IRaster above = Raster.CreateRaster(abovesavefilepath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            above.NoDataValue = -9999;
            above.ProjectionString = prj;
            above.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2.0, cellsize_x, 0, yllcenter + cellsize_x / 2.0, 0, cellsize_y });

            IRaster below = Raster.CreateRaster(belowsavefilepath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            below.NoDataValue = -9999;
            below.ProjectionString = prj;
            below.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2.0, cellsize_x, 0, yllcenter + cellsize_x / 2.0, 0, cellsize_y });

            int radius = 3;
            Kaido(src.Value, nrow - 1, ncol - 1, above.Value, below.Value, radius);

            above.Save();
            below.Save();
            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static private double todeg = 180 / Math.PI;

        static private void GetMinMax(IValueGrid src, int x, int y, int inc_x, int inc_y, double P, int range, out double fai, out double sai)
        {
            fai = 0;
            sai = 0;

            double za = src[y, x];
            double bata = double.NegativeInfinity;
            double gamma= double.PositiveInfinity;
            for(int i = 0; i < range; i++)
            {
                double zb = src[y + inc_y * i, x + inc_x * i];
                if (zb <= -9999) continue;

                double sita = Math.Atan((zb - za) / P) * todeg;
                bata = Math.Max(bata,  sita);
                gamma= Math.Min(gamma, sita);
            }

            fai = double.IsInfinity(fai) ? -9999: 90 - bata;
            sai = double.IsInfinity(sai) ? -9999: 90 + gamma;
        }


        static void Kaido(IValueGrid src, int nrow, int ncol, IValueGrid above, IValueGrid below, int radius)
        {
            double dx = 10.242;
            double dy = 12.369;

            double P_Vert = dy;
            double P_Horz = dx;
            double P_Diag = Math.Sqrt(Math.Pow(dy, 2) + Math.Pow(dx, 2));

            for (int x = 0; x <= ncol; x++)
                for (int y = 0; y <= nrow; y++)
                { 
                    above[y, x] = -9999;
                    below[y, x] = -9999;
                }

            int istart = radius - 1;
            int iendx = ncol - radius;
            int iendy = nrow - radius;
            for (int x = istart; x < iendx; x++)
            {
                for (int y = istart; y < iendy; y++)
                {
                    if (src[y, x] <= -9999) continue;
                    double[] fais = new double[8];
                    double[] sais = new double[8];

                    GetMinMax(src, x, y, 0, -1, P_Vert, radius, out fais[0], out sais[0]); // 0
                    GetMinMax(src, x, y, 1, -1, P_Diag, radius, out fais[1], out sais[1]); // 45
                    GetMinMax(src, x, y, 1,  0, P_Horz, radius, out fais[2], out sais[2]); // 90
                    GetMinMax(src, x, y, 1,  1, P_Diag, radius, out fais[3], out sais[3]); // 135
                    GetMinMax(src, x, y, 0,  1, P_Vert, radius, out fais[4], out sais[4]); // 180
                    GetMinMax(src, x, y, -1, 1, P_Diag, radius, out fais[5], out sais[5]); // 225
                    GetMinMax(src, x, y, -1, 0, P_Horz, radius, out fais[6], out sais[6]); // 270
                    GetMinMax(src, x, y, -1,-1, P_Diag, radius, out fais[7], out sais[7]); // 315

                    double cfai = 0, csai = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (fais[i] > -9999)
                            cfai+=1;
                        else
                            fais[i] = 0;

                        if (sais[i] > -9999)
                            csai+=1;
                        else
                            sais[i] = 0;
                    }

                    above[y, x] = (fais[0] + fais[1] + fais[2] + fais[3] + fais[4] + fais[5] + fais[6] + fais[7]) / cfai;
                    below[y, x] = (sais[0] + sais[1] + sais[2] + sais[3] + sais[4] + sais[5] + sais[6] + sais[7]) / csai;
                }
            }


        }
    }
}
