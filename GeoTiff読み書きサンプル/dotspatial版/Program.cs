using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;
using DotSpatial.Positioning;

namespace ConsoleApp1
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                SetDllDirectory(gdal_wrap.dllが置いてあるフォルダパス。例えば@"～\DotSpatial-master\Source\bin\x64\Release\gdal\x64"とか);

                string loadfilepath = 読み込むファイルパス;
                string savefilepath = 書き込みファイルパス;

                // GeoTiff読み込み
                GdalRasterProvider d = new GdalRasterProvider();
                IRaster src = d.Open(loadfilepath);
                int ncol = src.NumColumns; // 水平方向ピクセル数
                int nrow = src.NumRows; // 鉛直方向ピクセル数
                int band_num = src.NumBands; // バンド数
                string prj = src.ProjectionString;
                double nodata = src.NoDataValue;

                double[] pGT = src.Bounds.AffineCoefficients;
                double xllcenter = pGT[0];
                double cellsize_x = pGT[1];
                // pGT[2] # 回転 今回は使わない
                double yllcenter = pGT[3];
                // pGT[4] # 回転 今回は使わない
                double cellsize_y = pGT[5];

                IRaster dst = Raster.CreateRaster(savefilepath, src.DriverCode, ncol - 2, nrow - 2, 1, src.DataType, new[] { string.Empty });
                dst.NoDataValue = nodata;
                dst.ProjectionString = prj;
                dst.Bounds = new RasterBounds(nrow - 2, ncol - 2,
                    new double[] { xllcenter + cellsize_x / 2, cellsize_x, 0, yllcenter + cellsize_y / 2, 0, cellsize_y });

                Hoi(src.Value, nodata, xllcenter, yllcenter, cellsize_x, nrow, ncol, dst.Value);

                dst.Save();
            }
            finally
            {
                sw.Stop();
                Console.WriteLine(sw.Elapsed.ToString() + " 終了しました");
            }
        }

        static private void Hoi(IValueGrid src, double nodata, double xllcenter, double yllcenter, double cellsize, int nrow, int ncol, IValueGrid output)
        {
            double todeg = 180.0 / Math.PI;

            for (int i = 1; i < nrow - 1; i++)
            {
                bool flg = false;
                double dx3 = 0;
                double dy3 = 0;
                for (int j = 1; j < ncol - 1; j++)
                {
                    if (Check8(src, nodata, j, i) == false) continue;

                    if (flg == false)
                    {
                        Dxdy(j, i, xllcenter, yllcenter, cellsize, out dx3, out dy3);
                        dx3 = dx3 * 3;
                        dy3 = dy3 * 3;
                        flg = true;
                    }

                    double Hx = (src[i - 1, j - 1] + src[i, j - 1] + src[i + 1, j - 1] - (src[i - 1, j + 1] + src[i, j + 1] + src[i + 1, j + 1])) / (dx3);
                    double Hy = (src[i + 1, j - 1] + src[i + 1, j] + src[i + 1, j + 1] - (src[i - 1, j - 1] + src[i - 1, j] + src[i - 1, j + 1])) / (dy3);
                    double sita = Math.Atan(Hx / Hy) * todeg;
                    if (Hy < 0) sita = sita + 180;
                    output[i - 1, j - 1] = sita;
                    if ((Hy == 0) && (Hx == 0)) output[i - 1, j - 1] = nodata;
                }
            }
        }

        static private void Dxdy(int x, int y, double xcenter, double ycenter, double cellsize, out double dx, out double dy)
        {
            double centerlat = ycenter + cellsize * y;
            double centerlon = xcenter + cellsize * x;

            double leftlon = xcenter + cellsize * (x - 1);
            dx = LatLonDistance(centerlat, centerlon, centerlat, leftlon);

            double uplat = ycenter + cellsize * (y - 1);
            dy = LatLonDistance(centerlat, centerlon, uplat, centerlon);
        }

        static private double LatLonDistance(double lat1, double lon1, double lat2, double lon2)
        {
            Position position1 = new Position(new Latitude(lat1), new Longitude(lon1));
            Position position2 = new Position(new Latitude(lat2), new Longitude(lon2));
            Distance distance = position1.DistanceTo(position2);
            return distance.Value;
        }

        static private bool Check8(IValueGrid grd, double nodata, int x, int y)
        {
            if (grd[y - 1, x - 1] == nodata) return false;
            if (grd[y, x - 1] == nodata) return false;
            if (grd[y + 1, x - 1] == nodata) return false;

            if (grd[y, x - 1] == nodata) return false;
            if (grd[y, x + 1] == nodata) return false;

            if (grd[y - 1, x + 1] == nodata) return false;
            if (grd[y, x - 1] == nodata) return false;
            if (grd[y + 1, x + 1] == nodata) return false;
            return true;
        }
    }
}