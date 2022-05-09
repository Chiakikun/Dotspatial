using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace OneTanidozu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string abovefilepah = @"D:\地上開度.tif";
            string belowfilepah = @"D:\地下開度.tif";
            string savefilepath = @"D:\保存先.tif";

            SetDllDirectory(dllpath);

            // GeoTiff読み込み
            GdalRasterProvider d = new GdalRasterProvider();
            IRaster above = d.Open(abovefilepah);
            IRaster below = d.Open(belowfilepah);

            int ncol = above.NumColumns;
            int nrow = above.NumRows;
            int band_num = above.NumBands;
            string prj = above.ProjectionString;
            double nodata = above.NoDataValue;

            double[] pGT = above.Bounds.AffineCoefficients;
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

            OneTanidozu(above.Value, below.Value, nrow, ncol, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void OneTanidozu(IValueGrid above, IValueGrid below, int nrow, int ncol, IValueGrid dst)
        {
            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    dst[y, x] = -9999;

                    double az = above[y, x];
                    double bz = below[y, x];
                    if (az <= -9999 || bz <= -9999)
                        continue;

                    dst[y, x] = (az - bz) / 2.0;
                }
            }
            return;
        }

    }
}
