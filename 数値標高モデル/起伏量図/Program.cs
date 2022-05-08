using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Kifukuryozu
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
            dst.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_x / 2, 0, cellsize_y });

            Kifukuryo(src.Value, nrow - 1, ncol - 1, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Kifukuryo(IValueGrid src, int nrow, int ncol, IValueGrid dst)
        {
            int r = 5;    // 窓サイズ

            double[,] dsrc = new double[nrow, ncol];

            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                {
                    dst[y, x] = -9999;
                    dsrc[y, x] = src[y, x];
                }

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    // 窓領域内の最高標高、最低標高
                    double minelv = double.PositiveInfinity;
                    double maxelv = double.NegativeInfinity;

                    for (int m = x - r; m <= x + r; m++)
                    {
                        for (int n = y - r; n <= y + r; n++)
                        {
                            if ((dsrc[y, x] > -9999) && (m >= 0) && (n >= 0) && (m < ncol) && (n < nrow))
                            {
                                minelv = Math.Min(minelv, dsrc[n, m]);
                                maxelv = Math.Max(maxelv, dsrc[n, m]);
                            }
                        }
                    }

                    if (double.IsInfinity(minelv) || double.IsInfinity(maxelv))
                        continue;

                    dst[y, x] = maxelv - minelv;
                }
            }

            return;
        }
    }
}
