using System;
using System.Runtime.InteropServices;
using System.Linq;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Keishahenkansenzu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\DEM.tif";

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

            // 遷急線・遷緩線図
            string savefilepath = @"d:\遷急線・遷緩線図.tif";
            IRaster kankyu = Raster.CreateRaster(savefilepath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            kankyu.NoDataValue = nodata;
            kankyu.ProjectionString = prj;
            kankyu.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_y / 2, 0, cellsize_y });
            // 尾根・谷線図
            savefilepath = @"d:\尾根・谷線図.tif";
            IRaster onetani = Raster.CreateRaster(savefilepath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            onetani.NoDataValue = nodata;
            onetani.ProjectionString = prj;
            onetani.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_y / 2, 0, cellsize_y });

            Keishahenkansenzu(src.Value, nrow, ncol, kankyu.Value, onetani.Value);
            kankyu.Save();
            onetani.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Keishahenkansenzu(IValueGrid src, int nrow, int ncol, IValueGrid kankyu, IValueGrid onetani)
        {
            double dx = 8.763;
            double dy = 12.348;
            double dx2 = Math.Pow(dx, 2);
            double dy2 = Math.Pow(dy, 2);


            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    kankyu[y, x]  = -9999;
                    onetani[y, x] = -9999;

                    if ((x < 1) || (y < 1) || (x >= ncol - 1) || (y >= nrow - 1)) continue;
                    double H11 = src[y - 1, x - 1]; if (H11 == -9999) continue;
                    double H12 = src[y - 1, x];     if (H12 == -9999) continue;
                    double H13 = src[y - 1, x + 1]; if (H13 == -9999) continue;
                    double H21 = src[y, x - 1];     if (H21 == -9999) continue;
                    double H22 = src[y, x];         if (H21 == -9999) continue;
                    double H23 = src[y, x + 1];     if (H23 == -9999) continue;
                    double H31 = src[y + 1, x - 1]; if (H31 == -9999) continue;
                    double H32 = src[y + 1, x];     if (H32 == -9999) continue;
                    double H33 = src[y + 1, x + 1]; if (H33 == -9999) continue;

                    double tate = (H12 - 2 * H22 + H32) / dx2;              // |
                    double yoko = (H21 - 2 * H22 + H23) / dy2;              // ー
                    double naname1 = (H11 - 2 * H22 + H33) / (dx2 + dy2);   // ＼
                    double naname2 = (H13 - 2 * H22 + H31) / (dx2 + dy2);   // ／

                    double[] array_kyokuritsu = new double[] { Math.Abs(tate), Math.Abs(naname1), Math.Abs(yoko), Math.Abs(naname2) };

                    switch(array_kyokuritsu.ToList().IndexOf(array_kyokuritsu.Max()))
                    {
                        case 0:
                            if (((H12 <= H22 && H32 <= H22) || (H12 >= H22 && H32 >= H22)))
                                onetani[y, x] = tate;
                            else
                                kankyu[y, x] = tate;
                            break;
                        case 1:
                            if (((H11 <= H22 && H33 <= H22) || (H11 >= H22 && H33 >= H22)))
                                onetani[y, x] = naname1;
                            else
                                kankyu[y, x] = naname1;
                            break;
                        case 2:
                            if (((H21 <= H22 && H23 <= H22) || (H21 >= H22 && H23 >= H22)))
                                onetani[y, x] = yoko;
                            else
                                kankyu[y, x] = yoko;
                            break;
                        case 3:
                            if (((H13 <= H22 && H31 <= H22) || (H13 >= H22 && H31 >= H22)))
                                onetani[y, x] = naname2;
                            else
                                kankyu[y, x] = naname2;
                            break;
                    }

                    // この辺は好みに応じて調整する
                    if (-0.01 < onetani[y, x] && onetani[y, x] < 0.01) onetani[y, x] = -9999;
                    if (-0.01 < kankyu[y, x]  && kankyu[y, x]  < 0.01) kankyu[y, x]  = -9999;                    
                }
            }
            return;
        }
    }
}
