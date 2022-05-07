using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace Seppomenzu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\DEM.tif";
            string seppoupath =  @"D:\接峰面.tif";
            string sekkokupath = @"D:\接谷面.tif";

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
            IRaster seppou = Raster.CreateRaster(seppoupath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            seppou.NoDataValue = nodata;
            seppou.ProjectionString = prj;
            seppou.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_x / 2, 0, cellsize_y });
            IRaster sekkoku = Raster.CreateRaster(sekkokupath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            sekkoku.NoDataValue = nodata;
            sekkoku.ProjectionString = prj;
            sekkoku.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2, cellsize_x, 0, yllcenter - cellsize_x / 2, 0, cellsize_y });
            Seppomen(src.Value, nrow - 1, ncol - 1, seppou.Value, sekkoku.Value);
            seppou.Save();
            sekkoku.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Seppomen(IValueGrid src, int nrow, int ncol, IValueGrid seppou, IValueGrid sekkoku)
        {
            int r = 5;    // 窓サイズ
            int cal = 20; // 計算回数

            double[,] targetsep = new double[nrow, ncol];
            double[,] targetsek = new double[nrow, ncol];

            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                {
                    targetsep[y, x] = src[y, x];
                    targetsek[y, x] = src[y, x];
                }

            for (int i = 0; i < cal; i++)
            {
                double[,] tmpseppou  = targetsep.Clone() as double[,];
                double[,] tmpsekkoku = targetsek.Clone() as double[,];

                for (int x = 0; x < ncol; x++)
                {
                    for (int y = 0; y < nrow; y++)
                    {
                        // 窓領域内の平均
                        double cgridsep = 0;
                        double sumsep = 0;
                        double cgridsek = 0;
                        double sumsek = 0;
                        for (int m = x - r; m <= x + r; m++)
                        {
                            for (int n = y - r; n <= y + r; n++)
                            {
                                if ((targetsep[y, x] > -9999) && (m >= 0) && (n >= 0) && (m < ncol) && (n < nrow) && (targetsep[n, m] > -9999))
                                {
                                    cgridsep += 1;
                                    sumsep += targetsep[n, m];
                                }
                                if ((targetsek[y, x] > -9999) && (m >= 0) && (n >= 0) && (m < ncol) && (n < nrow) && (targetsek[n, m] > -9999))
                                {
                                    cgridsek += 1;
                                    sumsek += targetsek[n, m];
                                }
                            }
                        }
                        double avg = sumsep / cgridsep;
                        tmpseppou[y, x]  = targetsep[y, x] <= avg ? avg : targetsep[y, x];
                        avg = sumsek / cgridsek;
                        tmpsekkoku[y, x] = targetsek[y, x] >= avg ? avg : targetsek[y, x];
                    }
                }

                targetsep = tmpseppou.Clone()  as double[,];
                targetsek = tmpsekkoku.Clone() as double[,];
            }

            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                {
                    seppou[y, x]  = targetsep[y, x];
                    sekkoku[y, x] = targetsek[y, x];
                }
        }
    }
}
