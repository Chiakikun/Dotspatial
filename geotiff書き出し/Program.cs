using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace RasterWriteSample
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            // gdal等必要なdllをまとめて参照したいので
            SetDllDirectory(@"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86");

            string savefilepath = @"D:\test.tif";

            int ncol = 3; // 水平方向ピクセル数
            int nrow = 3; // 鉛直方向ピクセル数
            double nodata = -9999;
            double cellsize = 10;
            double xllcorner = 20000;
            double yllcorner = -40000;

            // https://epsg.io/6677 から
            string prj =
                @"PROJCS[""unnamed"",
                    GEOGCS[""GRS 1980(IUGG, 1980)"",
                    DATUM[""unknown"",
                            SPHEROID[""GRS80"", 6378137, 298.257222101]],
                        PRIMEM[""Greenwich"", 0],
                        UNIT[""degree"", 0.0174532925199433]],
                    PROJECTION[""Transverse_Mercator""],
                    PARAMETER[""latitude_of_origin"", 36],
                    PARAMETER[""central_meridian"", 139.8333333333333],
                    PARAMETER[""scale_factor"", 0.9999],
                    PARAMETER[""false_easting"", 0],
                    PARAMETER[""false_northing"", 0],
                    UNIT[""Meter"", 1],
                    AUTHORITY[""epsg"", ""6677""]]";

            // GeoTiff書き出し
            GdalRasterProvider d = new GdalRasterProvider(); // 無意味なようで無意味ではないです。拡張子の登録しています
            IRaster dst = Raster.CreateRaster(savefilepath, null, ncol, nrow, 1, typeof(float), new[] { string.Empty });
            dst.NoDataValue = nodata;
            dst.ProjectionString = prj;
            dst.Bounds = new RasterBounds(nrow, ncol, new double[] { xllcorner, cellsize, 0, yllcorner, 0, -1 * cellsize });

            dst.Value[0, 0] = 32;   // H11
            dst.Value[0, 1] = 64;   // H12
            dst.Value[0, 2] = 128;  // H13
            dst.Value[1, 0] = 16;   // H21
            dst.Value[1, 1] = 0;    // H22
            dst.Value[1, 2] = 1;    // H23
            dst.Value[2, 0] = 8;    // H31
            dst.Value[2, 1] = 4;    // H32
            dst.Value[2, 2] = 2;    // H33

            dst.Save();

            return;
        }
    }
}


