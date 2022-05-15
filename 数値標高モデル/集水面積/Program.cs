using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;
using GeoAPI.Geometries;


namespace Shusuimenseki
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            string dllpath = @"D:\DotSpatial-master\Source\DotSpatial.Data.Rasters.GdalExtension\bin\x64\Debug\gdal\x64";
                // 32bitの場合はこっち @"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86";
            string loadfilepath = @"D:\DEM.tif";    // 窪地埋め済のDEM
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
            dst.NoDataValue = -9999;
            dst.ProjectionString = prj;
            dst.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcenter - cellsize_x / 2.0, cellsize_x, 0, yllcenter + cellsize_y / 2.0, 0, cellsize_y });

            Shusuimenseki(src.Value, nrow, ncol, dst.Value);

            double m = Math.Pow(cellsize_x, 2);

            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                {
                    if (dst.Value[y, x] <= -9999)
                        continue;
                    dst.Value[y, x] *= m;
                }
            dst.Save();

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }


        static void Shusuimenseki(IValueGrid src, int nrow, int ncol, IValueGrid dst)
        {
            // 初期化
            double[,] dsrc = new double[nrow, ncol];
            for (int x = 0; x < ncol; x++)
                for (int y = 0; y < nrow; y++)
                {
                    dst[y, x] = 1.0;
                    dsrc[y, x] = src[y, x];
                }

            // 標高順にソート
            List<Coordinate> crds = new List<Coordinate>(nrow * ncol);
            for (int x = 0; x < ncol; x++)
            {
                for (int y = 0; y < nrow; y++)
                {

                    double z = dsrc[y, x];
                    if (z <= -9999)
                        continue;

                    crds.Add(new Coordinate(x, y, z));
                }
            }
            crds.Sort((a, b) => b.Z > a.Z ? 1 : b.Z < a.Z ? -1 : 0);

            // 計算
            for (int i = 0; i < crds.Count; i++)
            {
                int x = (int)crds[i].X;
                int y = (int)crds[i].Y;
                DPAREA(x, y, dsrc, nrow, ncol, dst, new Dictionary<Tuple<int, int, double>, int>(10000));
            }

            return;
        }


        static private int[,] e = new int[,] { { 1, 1 }, { 0, 1 }, { -1, 1 }, { 1, 0 }, { -1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 } };

        static void DPAREA(int x, int y, double[,] src, int nrow, int ncol, IValueGrid dst, Dictionary<Tuple<int, int, double>, int> _known)
        {
           Tuple<int, int, double> tmp = new Tuple<int, int, double>(x, y, src[y, x]);
            if (_known.ContainsKey(tmp))
                return;
            _known.Add(tmp, 0);

            double min_z = double.PositiveInfinity;
            int min_idx = -1;
            for (int i = 0; i < 8; i++)
            {
                int ex = x + e[i, 0];
                int ey = y + e[i, 1];

                if (ex < 1 || ex >= ncol - 1 || ey < 1 || ey >= nrow - 1)
                    continue; ;

                if (src[ey, ex] <= -9999 || src[y, x] <= src[ey, ex] || min_z <= src[ey, ex])
                    continue;

                min_z = src[ey, ex];
                min_idx = i;
            }
            if (min_idx < 0)
                return;

            dst[y + e[min_idx, 1], x + e[min_idx, 0]] += 1.0;
            DPAREA(x + e[min_idx, 0], y + e[min_idx, 1], src, nrow, ncol, dst, _known);
        }

    }
}
