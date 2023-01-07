using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Laplacianzu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\DEM.tif";
            string savelappath = @"D:\ラプラシアン.tif";
            string saveedgpath = @"D:\エッジ.tif";

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
            IRaster lap = Raster.CreateRaster(savelappath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            lap.NoDataValue = nodata;
            lap.ProjectionString = prj;
            lap.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_y / 2, 0, cellsize_y });
            IRaster edg = Raster.CreateRaster(saveedgpath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            edg.NoDataValue = nodata;
            edg.ProjectionString = prj;
            edg.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_y / 2, 0, cellsize_y });

            Laplacianzu(src.Value, nrow, ncol, lap.Value, edg.Value);

            lap.Save();
            edg.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Laplacianzu(IValueGrid src, int nrow, int ncol, IValueGrid lap, IValueGrid edg)
        {
            double dx = 8.763;
            double dy = 12.348;

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    lap[y, x] = -9999;
                    edg[y, x] = -9999;

                    if ((x < 1) || (y < 1) || (x >= ncol - 1) || (y >= nrow - 1)) continue;
                    double H11 = src[y - 1, x - 1]; if (H11 == -9999) continue;
                    double H12 = src[y - 1, x];     if (H12 == -9999) continue;
                    double H13 = src[y - 1, x + 1]; if (H13 == -9999) continue;
                    double H21 = src[y, x - 1];     if (H21 == -9999) continue;
                    double H22 = src[y, x];         if (H22 == -9999) continue;
                    double H23 = src[y, x + 1];     if (H23 == -9999) continue;
                    double H31 = src[y + 1, x - 1]; if (H31 == -9999) continue;
                    double H32 = src[y + 1, x];     if (H32 == -9999) continue;
                    double H33 = src[y + 1, x + 1]; if (H33 == -9999) continue;

                    double I = ((H21 + H23) - 2 * H22) / Math.Pow(dx, 2) + ((H12 + H32) - 2 * H22) / Math.Pow(dy, 2);

                    lap[y, x] = I;
                    edg[y, x] = Math.Abs(I);
                }
            }
            return;
        }
    }
}
