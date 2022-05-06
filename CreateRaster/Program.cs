using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;
using DotSpatial.Positioning;
using GeoAPI.Geometries;

namespace CreateRaster
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            SetDllDirectory(@"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86");

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                double cellsize = 10;
                string fieldname = "ラスタに投入する値が入っているフィールド";
                string inputfile = @"D:\pnt.shp";
                string outputfile = "";

                // shp読み込み
                Shapefile shp = Shapefile.OpenFile(inputfile);
                int nX = (int)Math.Truncate((shp.Extent.MaxX - shp.Extent.MinX) / cellsize);
                int nY = (int)Math.Truncate((shp.Extent.MaxY - shp.Extent.MinY) / cellsize);

                // ラスタ作成
                GdalRasterProvider d = new GdalRasterProvider();
                IRaster dst = Raster.CreateRaster(outputfile, null, nX, nY, 1, typeof(float), new[] { string.Empty });
                dst.NoDataValue = -9999;
                dst.ProjectionString = shp.ProjectionString;
                dst.Bounds = new RasterBounds(nY, nX, new double[] { shp.Extent.MinX - cellsize / 2, cellsize, 0, shp.Extent.MinY - cellsize / 2, 0, cellsize });
                for (int x = 0; x < nX; x++)
                    for (int y = 0; y < nY; y++)
                        dst.Value[y, x] = -9999;

                // 値投入
                System.Data.DataTable dt = shp.DataTable;
                int idxcol = dt.Columns.IndexOf(fieldname);
                int n = shp.NumRows();
                for (int i = 0; i < n; i++)
                {
                    IGeometry geo = shp.GetFeature(i).Geometry;
                    Coordinate[] crd = geo.Coordinates;
                    for (int j = 0; j < crd.Length; j++)
                    {
                        int idxx = (int)Math.Truncate((crd[j].X - shp.Extent.MinX) / cellsize);
                        int idxy = (int)Math.Truncate((crd[j].Y - shp.Extent.MinY) / cellsize);
                        dst.Value[idxy, idxx] = (double)dt.Rows[i][idxcol];
                    }
                }

                dst.Save();
            }
            finally
            {
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }

            Console.WriteLine("終了しました。");
            Console.ReadKey();
            return;
        }
    }
}