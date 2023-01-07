using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Texturezu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\尾根谷図.tif"; // 尾根谷図の出力を使う
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

            Texture(src.Value, nrow, ncol, dst.Value);

            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static bool isTaniOne(double value)
        {
            double tani = 1;
            double one = -1;

            return ((one < value) && (value < tani)) ? false : true;
        }


        static void Texture(IValueGrid gsrc, int nrow, int ncol, IValueGrid dst)
        {
            int r = 10;

            double[,] src = new double[nrow, ncol];
            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                    src[y, x] = gsrc[y, x];

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    dst[y, x] = -9999;
                    if (src[y, x] == -9999)
                    {
                        if (src[y, x] <= -9999)
                            src[y, x] = 0;
                        else
                            src[y, x] = gsrc[y, x];
                    }

                    int count = 0;

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
                                if(isTaniOne(src[y + fluct_y, x + fluct_x]))
                                    count++;
                            }

                            // 第2象限
                            fluct_x = x_axis * (-1);
                            fluct_y = i;
                            if (fluct_x != 0) // 第1象限で計算済だから
                            {
                                if ((x + fluct_x >= 0) && (y + fluct_y >= 0) && (x + fluct_x < ncol) && (y + fluct_y < nrow))
                                {
                                    if (isTaniOne(src[y + fluct_y, x + fluct_x]))
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
                                    if (isTaniOne(src[y + fluct_y, x + fluct_x]))
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
                                    if (isTaniOne(src[y + fluct_y, x + fluct_x]))
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