using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Kyokuritsuzu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\DEM.tif";
            string savefilepath = @"D:\保存先.tif";

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
            IRaster dst = Raster.CreateRaster(savefilepath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            dst.NoDataValue = nodata;
            dst.ProjectionString = prj;
            dst.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_y / 2, 0, cellsize_y });

            Kyokuritsuzu(src.Value, nrow, ncol, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Kyokuritsuzu(IValueGrid src, int nrow, int ncol, IValueGrid dst)
        {
            double dx = 8.763;
            double dy = 12.348;

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    dst[y, x] = -9999;

                    if ((x < 1) || (y < 1) || (x >= ncol - 1) || (y >= nrow - 1)) continue; // srcより1周り小さい範囲内でなければ計算できない
                    double H11 = src[y - 1, x - 1]; if (H11 == -9999) continue;
                    double H12 = src[y - 1, x];     if (H12 == -9999) continue;
                    double H13 = src[y - 1, x + 1]; if (H13 == -9999) continue;
                    double H21 = src[y, x - 1];     if (H21 == -9999) continue;
                    double H22 = src[y, x];         if (H22 == -9999) continue;
                    double H23 = src[y, x + 1];     if (H23 == -9999) continue;
                    double H31 = src[y + 1, x - 1]; if (H31 == -9999) continue;
                    double H32 = src[y + 1, x];     if (H32 == -9999) continue;
                    double H33 = src[y + 1, x + 1]; if (H33 == -9999) continue;

                    double dx2 = Math.Pow(dx, 2);
                    double dy2 = Math.Pow(dy, 2);
                    double Kx = (H23 - H21) / (2 * dx);
                    double Ky = (H32 - H12) / (2 * dy);
                    double Kx2 = Math.Pow(Kx, 2);
                    double Ky2 = Math.Pow(Ky, 2);
                    double Kxx = (H21 + H23 - 2 * H22) / dx2;
                    double Kyy = (H12 + H32 - 2 * H22) / dy2;
                    double Kxy = (H31 - H13 - H33 + H11) / (2 * (dx2 + dy2));
                    double K = (Kxx * (1+ Ky2) + Kyy * (1+Kx2) -2 * Kx * Ky * Kxy) / (2 * Math.Pow(1+Kx2+Ky2, 3 / 2));
 
                    dst[y, x] = (K < 0 ? Math.Sqrt(Math.Abs(K)) * -1 : Math.Sqrt(K)) * 100;
                }
            }
            return;
        }
    }
}
