﻿using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Kobaizu
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

            Kobaizu(src.Value, nrow, ncol, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Kobaizu(IValueGrid src, int nrow, int ncol, IValueGrid dst)
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
                    double H12 = src[y - 1, x]; if (H12 == -9999) continue;
                    double H13 = src[y - 1, x + 1]; if (H13 == -9999) continue;
                    double H21 = src[y, x - 1]; if (H21 == -9999) continue;
                    double H23 = src[y, x + 1]; if (H23 == -9999) continue;
                    double H31 = src[y + 1, x - 1]; if (H31 == -9999) continue;
                    double H32 = src[y + 1, x]; if (H32 == -9999) continue;
                    double H33 = src[y + 1, x + 1]; if (H33 == -9999) continue;

                    double Ix = ((H11 + 2 * H21 + H31) - (H13 + 2 * H23 + H33)) / (8 * dx);
                    double Iy = ((H11 + 2 * H12 + H13) - (H31 + 2 * H32 + H33)) / (8 * dy);
                    double I = Math.Pow(Math.Atan(Math.Pow(Ix, 2) + Math.Pow(Iy, 2)), 0.5) * 180 / Math.PI;

                    dst[y, x] = I;
                }
            }
            return;
        }
    }
}
