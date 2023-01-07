using System;
using System.Runtime.InteropServices;
using System.Linq;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

using System.Collections.Generic;

namespace KodoBunsanryoIjouzu
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loaddempath =  @"D:\DEM.tif";
            string loadkodopath = @"D:\高度分散量図.tif";
            string savefilepath = @"D:\保存先.tif";

            SetDllDirectory(dllpath);

            // GeoTiff読み込み
            GdalRasterProvider d = new GdalRasterProvider();
            IRaster dem = d.Open(loaddempath);
            IRaster kod = d.Open(loadkodopath);

            int ncol = dem.NumColumns;
            int nrow = dem.NumRows;
            int band_num = dem.NumBands;
            string prj = dem.ProjectionString;
            double nodata = dem.NoDataValue;

            double[,] avgKodoBunsan = AvgKodoBunsanryo(dem.Value, kod.Value, nrow, ncol);
            double [] keisu = GetKinjiKeisu(avgKodoBunsan);

            double[] pGT = dem.Bounds.AffineCoefficients;
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

            Ijouzu(dem.Value, kod.Value, nrow, ncol, dst.Value, keisu);
            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }

        static double[,] AvgKodoBunsanryo(IValueGrid dem, IValueGrid kodo, int nrow, int ncol)
        {
            List<double>[] avgs = new List<double>[29]; // 幌延町だとこのくらい必要
            for (int i = 0; i < avgs.Length; i++)
                avgs[i] = new List<double>();

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    if (dem[y, x] <= -9999)
                        continue;
                    int idx = (int)dem[y, x] / 25;
                    avgs[idx].Add(kodo[y, x]);
                }
            }

            double[,] ret = new double[avgs.Length, 2];
            for (int i = 0; i < avgs.Length; i++)
            {
                ret[i, 0] = i * 25;
                ret[i, 1] = avgs[i].Sum() / avgs[i].Count;
            }
            return ret;
        }


        static public double[] GetKinjiKeisu(double[,] pnts)
        {
            double[,] Det_a = new double[4, 4];
            double[,] Inv_a = new double[4, 4];
            double[,] Det_b = new double[4, 1];
            double[,] Det_c = new double[4, 1];
            double buf;
            int m = 4;

            for (int i = 1; i < pnts.GetLength(0) - 2; i++) // 資料の見た目に合わせるために調整した
            {
                double x6 = Math.Pow(pnts[i, 0], 6);
                double x5 = Math.Pow(pnts[i, 0], 5);
                double x4 = Math.Pow(pnts[i, 0], 4);
                double x3 = Math.Pow(pnts[i, 0], 3);
                double x2 = Math.Pow(pnts[i, 0], 2);
                double x  = pnts[i, 0];

                Det_a[0, 0] += x6; Det_a[0, 1] += x5; Det_a[0, 2] += x4; Det_a[0, 3] += x3;
                Det_a[1, 0] += x5; Det_a[1, 1] += x4; Det_a[1, 2] += x3; Det_a[1, 3] += x2;
                Det_a[2, 0] += x4; Det_a[2, 1] += x3; Det_a[2, 2] += x2; Det_a[2, 3] += x;
                Det_a[3, 0] += x3; Det_a[3, 1] += x2; Det_a[3, 2] += x;  Det_a[3, 3] = 26;

                Det_b[0, 0] += x3 * pnts[i, 1];
                Det_b[1, 0] += x2 * pnts[i, 1];
                Det_b[2, 0] += x  * pnts[i, 1];
                Det_b[3, 0] += pnts[i, 1];
            }
            
            for (int i = 0; i < m; i++)
                for (int j = 0; j < m; j++)
                    Inv_a[i, j] = (i == j) ? 1.0 : 0.0;

            for (int i = 0; i < m; i++)
            {
                buf = 1 / Det_a[i, i];
                for (int j = 0; j < m; j++)
                {
                    Det_a[i, j] *= buf;
                    Inv_a[i, j] *= buf;
                }
                for (int j = 0; j < m; j++)
                    if (i != j)
                    {
                        buf = Det_a[j, i];
                        for (int k = 0; k < m; k++)
                        {
                            Det_a[j, k] -= Det_a[i, k] * buf;
                            Inv_a[j, k] -= Inv_a[i, k] * buf;
                        }
                    }
            }

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 1; j++)
                    for (int k = 0; k < 4; k++)
                        Det_c[i, j] += Inv_a[i, k] * Det_b[k, j];

            double[] result = new double[4];
            result[0] = Det_c[0, 0];
            result[1] = Det_c[1, 0];
            result[2] = Det_c[2, 0];
            result[3] = Det_c[3, 0];
            return result;
        }


        static void Ijouzu(IValueGrid dem, IValueGrid kodo, int nrow, int ncol, IValueGrid dst, double []keisu)
        {
            double a = keisu[0];
            double b = keisu[1];
            double c = keisu[2];
            double d = keisu[3];

            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {
                    if (dem[y, x] <= -9999) continue;

                    double kinji = a * Math.Pow(dem[y, x], 3) + b * Math.Pow(dem[y, x], 2) + c * dem[y, x] + d;
                    dst[y, x] = kodo[y, x] - kinji;
                }
            }
            return;
        }
    }
}
