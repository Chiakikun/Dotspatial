using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using OSGeo.GDAL; //gdal_csharpを参照させる
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

                Gdal.AllRegister();

                // GeoTiff読み込み
                Dataset ds = Gdal.Open(loadfilepath, Access.GA_Update);
                int ncol = ds.RasterXSize; // 水平方向ピクセル数
                int nrow = ds.RasterYSize; // 鉛直方向ピクセル数
                int band_num = ds.RasterCount; // バンド数
                string prj = ds.GetProjection();

                double nodata;
                int hasval;
                ds.GetRasterBand(1).GetNoDataValue(out nodata, out hasval);

                double[] pGT = new double[6];
                ds.GetGeoTransform(pGT);
                double xllcorner = pGT[0];
                double cellsize_x = pGT[1];
                // pGT[2] # 回転 今回は使わない
                double yllcorner = pGT[3];
                // pGT[4] # 回転 今回は使わない
                double cellsize_y = pGT[5];

                double[][] dVals = new double[band_num][];
                for (int i = 1; i <= band_num; i++)
                {
                    dVals[i - 1] = new double[ncol * nrow];
                    ds.GetRasterBand(i).ReadRaster(0, 0, ncol, nrow, dVals[i - 1], ncol, nrow, 0, 0);
                }

                double xllcenter = xllcorner + cellsize_x / 2;
                double yllcenter = yllcorner + cellsize_y / 2;
                double[] output = Hoi(dVals[0], nodata, xllcenter, yllcenter, cellsize_x, nrow, ncol);

                // GeoTiff書き出し
                ds = null;
                Driver imgDriver = Gdal.GetDriverByName("GTiff");
                ds = imgDriver.Create(savefilepath, ncol - 2, nrow - 2, 1, DataType.GDT_Float32, new string[] { });
                ds.SetProjection(prj);
                ds.SetGeoTransform(new double[] { xllcorner + cellsize_x, cellsize_x, 0, yllcorner - cellsize_x, 0, -1 * cellsize_x });
                Band rband = ds.GetRasterBand(1);
                rband.WriteRaster(0, 0, ncol - 2, nrow - 2, output, ncol - 2, nrow - 2, 0, 0);
                rband.SetNoDataValue(nodata);
                ds.FlushCache();

                return;
            }
            finally
            {
                sw.Stop();
                Console.WriteLine(sw.Elapsed.ToString() + " 終了しました");
            }
        }

        static private double[] Hoi(double[] dem, double nodata, double xllcenter, double yllcenter, double cellsize, int nrow, int ncol)
        {
            double[] output = new double[(nrow - 2) * (ncol - 2)];

            double todeg = 180.0 / Math.PI;

            for (int i = 1; i < nrow - 1; i++)
            {
                bool flg = false;
                double dx3 = 0;
                double dy3 = 0;
                for (int j = 1; j < ncol - 1; j++)
                {
                    if (Check8(dem, nodata, j, i, ncol) == false) continue;

                    if (flg == false)
                    {
                        Dxdy(j, i, xllcenter, yllcenter, cellsize, out dx3, out dy3);
                        dx3 = dx3 * 3;
                        dy3 = dy3 * 3;
                        flg = true;
                    }

                    double Hx = (dem[(i - 1) * ncol + (j - 1)] + dem[i * ncol + (j - 1)] + dem[(i + 1) * ncol + (j - 1)] - (dem[(i - 1) * ncol + (j + 1)] + dem[i * ncol + (j + 1)] + dem[(i + 1) * ncol + (j + 1)])) / (dx3);
                    double Hy = (dem[(i + 1) * ncol + (j - 1)] + dem[(i + 1) * ncol + j] + dem[(i + 1) * ncol + (j + 1)] - (dem[(i - 1) * ncol + (j - 1)] + dem[(i - 1) * ncol + j] + dem[(i - 1) * ncol + (j + 1)])) / (dy3);
                    double sita = Math.Atan(Hx / Hy) * todeg;
                    if (Hy < 0) sita = sita + 180;
                    output[(i - 1) * (ncol - 2) + (j - 1)] = sita;
                    if ((Hy == 0) && (Hx == 0)) output[(i - 1) * (ncol - 2) + (j - 1)] = nodata;
                }
            }
            return output;
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

        static private bool Check8(double[] dem, double nodata, int x, int y, int ncol)
        {
            if (dem[(x - 1) + (y + 1) * ncol] == nodata) return false;
            if (dem[x + (y + 1) * ncol] == nodata) return false;
            if (dem[(x + 1) + (y + 1) * ncol] == nodata) return false;

            if (dem[(x - 1) + y * ncol] == nodata) return false;
            if (dem[(x + 1) + y * ncol] == nodata) return false;

            if (dem[(x - 1) + (y - 1) * ncol] == nodata) return false;
            if (dem[x + (y - 1) * ncol] == nodata) return false;
            if (dem[(x + 1) + (y - 1) * ncol] == nodata) return false;

            return true;
        }
    }
}
