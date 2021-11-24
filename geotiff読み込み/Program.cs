using System;
using System.Runtime.InteropServices;

using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;

namespace RasterReadSample
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SetDllDirectory(string lpPathName);

        static void Main(string[] args)
        {
            SetDllDirectory(@"D:\DotSpatial-master\Source\bin\Debug\Windows Extensions\DotSpatial.Data.Rasters.GdalExtension\gdal\x86");

            string loadfilepath = @"D:\test.tif";

            // GeoTiff読み込み
            GdalRasterProvider d = new GdalRasterProvider();
            IRaster src = d.Open(loadfilepath);

            int ncol =      src.NumColumns;
            int nrow =      src.NumRows;
            int band_num =  src.NumBands;
            string prj =    src.ProjectionString;
            double nodata = src.NoDataValue;

            double[] pGT = src.Bounds.AffineCoefficients;
            double xllcenter =  pGT[0]; // コーナーではないので注意
            double cellsize_x = pGT[1];
            double rotate1 =    pGT[2];
            double yllcenter =  pGT[3]; // コーナーではないので注意
            double rotate2 =    pGT[4];
            double cellsize_y = pGT[5];

            Console.WriteLine("H11 = " + src.Value[0, 0]);
            Console.WriteLine("H12 = " + src.Value[0, 1]);
            Console.WriteLine("H13 = " + src.Value[0, 2]);
            Console.WriteLine("H21 = " + src.Value[1, 0]);
            Console.WriteLine("H22 = " + src.Value[1, 1]);
            Console.WriteLine("H23 = " + src.Value[1, 2]);
            Console.WriteLine("H31 = " + src.Value[2, 0]);
            Console.WriteLine("H32 = " + src.Value[2, 1]);
            Console.WriteLine("H33 = " + src.Value[2, 2]);

            Console.ReadKey();
            return;
        }
    }
}