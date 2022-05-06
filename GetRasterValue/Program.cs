using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;
using DotSpatial.Positioning;
using GeoAPI.Geometries;
using DotSpatial.Projections;

namespace GetRasterValue
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            SetDllDirectory(@"D:\DotSpatial-master\Source\DotSpatial.Data.Rasters.GdalExtension\bin\x64\Debug\gdal\x64");
            string inputfile = @"D:\GeoTiff.tif";
            string outputfile =@"D:\Pnt.shp";
            string field = "field"; // tifの値を投入するフィールド

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                // GeoTiff読み込み
                GdalRasterProvider d = new GdalRasterProvider();
                IRaster src = d.Open(inputfile);

                int ncol = src.NumColumns;
                int nrow = src.NumRows;
                int band_num = src.NumBands;
                string prj = src.ProjectionString;
                double nodata = src.NoDataValue;

                double[] pGT = src.Bounds.AffineCoefficients;
                double xllcenter = pGT[0];
                double cellsize_x = pGT[1];
                //            double rotate1 = pGT[2];
                double yllcenter = pGT[3];
                //            double rotate2 = pGT[4];
                double cellsize_y = pGT[5];

                Shapefile shp = Shapefile.OpenFile(outputfile);
                ProjectionInfo pdst = ProjectionInfo.FromEsriString(src.Projection.ToEsriString());
                shp.Reproject(pdst);

                System.Data.DataTable dt = shp.DataTable;
                int colindex = dt.Columns.IndexOf(field);

                int n = shp.NumRows();
                for (int i = 0; i < n; i++)
                {
                    IGeometry geo = shp.GetFeature(i).Geometry;
                    //IGeometry geo = shp.Features[i].Geometry;

                    Coordinate[] crd = geo.Coordinates;
                    for (int j = 0; j < crd.Length; j++)
                    {
                        double value = GetRasterValue(src, crd[j].X, crd[j].Y);

                        if (0 < value)
                            dt.Rows[i][colindex] = value;
                        else
                            dt.Rows[i][colindex] = -9999;
                    }
                }

                shp.Save();
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

        static double GetRasterValue(IRaster src, double x, double y)
        {
            int idxx = (int)((x - src.Extent.MinX) / src.CellWidth);
            int idxy = (int)((src.Extent.MaxY - y) / src.CellHeight);
            if ((idxx < 0) || (idxy < 0))
                return double.NaN;
            if ((idxx > src.NumColumns - 1) || (idxy > src.NumRows - 1))
                return double.NaN;

            double pixgeomx = src.CellWidth * idxx + src.Extent.MinX + src.CellWidth / 2;
            double pixgeomy = src.Extent.MaxY - src.CellHeight * idxy - src.CellHeight / 2;

            double[] a, b, c, d;
            // 右上
            if ((pixgeomx < x) && (pixgeomy < y))
            {
                if ((idxx > src.NumColumns - 2) || (idxy - 1 < 0))
                    return double.NaN;
                a = getvalue(src, idxx + 1, idxy - 1);
                b = getvalue(src, idxx, idxy - 1);
                c = getvalue(src, idxx, idxy);
                d = getvalue(src, idxx + 1, idxy);
            }
            // 左上
            else if ((pixgeomx >= x) && (pixgeomy < y))
            {
                if ((idxx - 1 < 0) || (idxy - 1 < 0))
                    return double.NaN;
                a = getvalue(src, idxx, idxy - 1);
                b = getvalue(src, idxx - 1, idxy - 1);
                c = getvalue(src, idxx - 1, idxy);
                d = getvalue(src, idxx, idxy);
            }
            // 左下
            else if ((pixgeomx >= x) && (pixgeomy >= y))
            {
                if ((idxx - 1 < 0) || (idxy > src.NumRows - 2))
                    return double.NaN;
                a = getvalue(src, idxx, idxy);
                b = getvalue(src, idxx - 1, idxy);
                c = getvalue(src, idxx - 1, idxy + 1);
                d = getvalue(src, idxx, idxy + 1);
            }
            // 右下
            else if ((pixgeomx < x) && (pixgeomy >= y))
            {
                if ((idxx > src.NumColumns - 2) || (idxy > src.NumRows - 2))
                    return double.NaN;
                a = getvalue(src, idxx + 1, idxy);
                b = getvalue(src, idxx, idxy);
                c = getvalue(src, idxx, idxy + 1);
                d = getvalue(src, idxx + 1, idxy + 1);
            }
            else
                return double.NaN;
                        
            double dist1 = LatLonDistance(b[1], b[0], a[1], a[0]);
            double dist2 = LatLonDistance(b[1], b[0], b[1], x);
            double delta1 = (a[2] - b[2]) / dist1;
            double tmp1 = delta1 * dist2 + b[2];

            double dist3 = LatLonDistance(c[1], c[0], d[1], d[0]);
            double dist4 = LatLonDistance(c[1], c[0], c[1], x);
            double delta2 = (d[2] - c[2]) / dist3;
            double tmp2 = delta2 * dist4 + c[2];

            double dist5 = LatLonDistance(d[1], d[0], a[1], a[0]);
            double dist6 = LatLonDistance(d[1], d[0], y,    d[0]);
            double delta3 = (tmp1 - tmp2) / dist5;

            return delta3 * dist6 + tmp2;
        }

        static private double[] getvalue(IRaster rst, int idxx, int idxy)
        {
            double x = rst.CellWidth * idxx + rst.Extent.MinX + rst.CellWidth / 2;
            double y = rst.Extent.MaxY - rst.CellHeight * idxy - rst.CellHeight / 2;
            double z = rst.Value[idxy, idxx];
            return new double[] { x, y, z};
        }

        static private double LatLonDistance(double lat1, double lon1, double lat2, double lon2)
        {
            Position position1 = new Position(new Latitude(lat1), new Longitude(lon1));
            Position position2 = new Position(new Latitude(lat2), new Longitude(lon2));
            Distance distance = position1.DistanceTo(position2);

            if (distance.Units == DistanceUnit.Centimeters)
                return distance.Value / 10;
            else
                return distance.Value;
        }
    }
}