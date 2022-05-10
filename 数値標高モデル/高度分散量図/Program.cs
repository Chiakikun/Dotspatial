using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

using System.Collections.Generic;

namespace KodoBunsanryozu
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

            KodoBunsanryo(src.Value, nrow, ncol, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void KodoBunsanryo(IValueGrid src, int nrow, int ncol, IValueGrid dst)
        {
            int r = 5;    // 窓サイズ

            double[,] dsrc = new double[nrow, ncol];
            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                    dsrc[y, x] = src[y, x];

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    dst[y, x] = -9999;

                    if (dsrc[y, x] <= -9999)
                        continue;

                    // 窓領域内の平均
                    double cgrid = 0;
                    double sum = 0;
                    for (int m = x - r; m <= x + r; m++)
                    {
                        for (int n = y - r; n <= y + r; n++)
                        {
                            if ((m >= 0) && (n >= 0) && (m < ncol) && (n < nrow) && (dsrc[n, m] > -9999))
                            {
                                sum += dsrc[n, m];
                                cgrid += 1;
                            }
                        }
                    }
                    double avg = sum / cgrid;

                    // 偏差
                    double dev = 0;
                    for (int m = x - r; m <= x + r; m++)
                    {
                        for (int n = y - r; n <= y + r; n++)
                            if ((m >= 0) && (n >= 0) && (m < ncol) && (n < nrow) && (dsrc[n, m] > -9999))
                                dev += Math.Pow(dsrc[n, m] - avg, 2);
                    }
                    dev = Math.Sqrt(dev / cgrid);

                    dst[y, x] = dev;
                }
            }

            return;
        }
    }
}
