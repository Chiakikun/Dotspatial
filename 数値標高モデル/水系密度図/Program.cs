using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Suikeimitsudozu
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

            Suikeimitsudo(src.Value, nrow - 1, ncol - 1, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Suikeimitsudo(IValueGrid src, int nrow, int ncol, IValueGrid dst)
        {
            int r = 7;

            for (int x = 0; x <= ncol; x++)
                for (int y = 0; y <= nrow; y++)
                    dst[y, x] = -9999;

            for (int x = 1; x < ncol; x++)
            {
                for (int y = 1; y < nrow; y++)
                {
                    if (src[y, x] == -9999)
                        continue;

                    int count = 0;

                    double tani = -1;  // 適当に調整

                    int fluct_x = 0, fluct_y = 0;
                    for (int x_axis = 0; x_axis <= r; x_axis++)
                    {
                        int y_axis = (int)Math.Truncate(Math.Sqrt(Math.Pow(r, 2) - Math.Pow(x_axis, 2)));
                        for (int i = 0; i <= y_axis; i++)
                        {
                            // 第1象限
                            fluct_x = x_axis;
                            fluct_y = i;
                            if ((x + fluct_x >= 0) && (y + fluct_y >= 0) && (x + fluct_x < ncol) && (y + fluct_y < nrow))
                            {
                                if ((src[y + fluct_y, x + fluct_x] <= tani) && (src[y + fluct_y, x + fluct_x] > -9999))
                                    count++;
                            }

                            // 第2象限
                            fluct_x = x_axis * (-1);
                            fluct_y = i;
                            if(fluct_x != 0) // 第1象限で計算済だから
                            {
                                if ((x + fluct_x >= 0) && (y + fluct_y >= 0) && (x + fluct_x < ncol) && (y + fluct_y < nrow))
                                {
                                    if ((src[y + fluct_y, x + fluct_x] <= tani) && (src[y + fluct_y, x + fluct_x] > -9999))
                                        count++;
                                }
                            }

                            // 第3象限
                            fluct_x = x_axis * (-1);
                            fluct_y = i * (-1);
                            if (fluct_y != 0) // 第2象限で計算済だから
                            {
                                if ((x + fluct_x >= 0) && (y + fluct_y >= 0) && (x + fluct_x < ncol) && (y + fluct_y < nrow))
                                {
                                    if ((src[y + fluct_y, x + fluct_x] <= tani) && (src[y + fluct_y, x + fluct_x] > -9999))
                                        count++;
                                }
                            }

                            // 第4象限
                            fluct_x = x_axis;
                            fluct_y = i * (-1);
                            if ((fluct_y != 0) && (fluct_x != 0)) // 第1、第3象限で計算済だから
                            {
                                if ((x + fluct_x >= 0) && (y + fluct_y >= 0) && (x + fluct_x < ncol) && (y + fluct_y < nrow))
                                {
                                    if ((src[y + fluct_y, x + fluct_x] <= tani) && (src[y + fluct_y, x + fluct_x] > -9999))
                                        count++;
                                }
                            }
                        }
                    }


                    dst[y, x] = count;
                }
            }
            return;
        }
    }
}