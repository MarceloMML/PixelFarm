﻿//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Mini
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            PixelFarm.Platforms.StorageService.RegisterProvider(new YourImplementation.LocalFileStorageProvider(""));

            //2.2 Icu Text Break info
            //test Typography's custom text break,
            //check if we have that data?            
            //------------------------------------------- 
            //string typographyDir = @"brkitr_src/dictionaries";
            string icu_datadir = YourImplementation.RelativePathBuilder.SearchBackAndBuildFolderPath(System.IO.Directory.GetCurrentDirectory(), "PixelFarm", @"..\Typography\Typography.TextBreak\icu62\brkitr");
            if (!System.IO.Directory.Exists(icu_datadir))
            {
                throw new System.NotSupportedException("dic");
            }
            var dicProvider = new Typography.TextBreak.IcuSimpleTextFileDictionaryProvider() { DataDir = icu_datadir };
            Typography.TextBreak.CustomBreakerBuilder.Setup(dicProvider);
            YourImplementation.TestBedStartup.Setup();


            //register image loader
            Mini.DemoHelper.RegisterImageLoader(LoadImage);


            //default text breaker, this bridge between              
#if DEBUG
            //PixelFarm.Agg.ActualImage.InstallImageSaveToFileService((IntPtr imgBuffer, int stride, int width, int height, string filename) =>
            //{

            //    using (System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //    {
            //        PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(imgBuffer, newBmp);
            //        //save
            //        newBmp.Save(filename);
            //    }
            //});
#endif



            //---------------------------------------------------
            //register image loader
            Mini.DemoHelper.RegisterImageLoader(LoadImage);
            //---------------------------- 
            //app specfic
            RootDemoPath.Path = @"..\Data";//*** 
            //----------------------------
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDev());
        }
        static PixelFarm.CpuBlit.MemBitmap LoadImage(string filename)
        {


            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(filename);


            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                       System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                       System.Drawing.Imaging.PixelFormat.Format32bppArgb //lock and read as 32-argb
                                       );

            PixelFarm.CpuBlit.MemBitmap memBmp = new PixelFarm.CpuBlit.MemBitmap(bmp.Width, bmp.Height);
            unsafe
            {
                var ptrBuffer = PixelFarm.CpuBlit.MemBitmap.GetBufferPtr(memBmp);
                PixelFarm.CpuBlit.MemMx.memcpy((byte*)ptrBuffer.Ptr, (byte*)bmpData.Scan0, bmp.Width * 4 * bmp.Height);
            }


            //int[] imgBuffer = new int[bmpData.Width * bmp.Height];
            //System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, imgBuffer, 0, imgBuffer.Length);
            bmp.UnlockBits(bmpData);

            //gdi+ load as little endian

            memBmp.IsBigEndian = false;
            bmp.Dispose();
            return memBmp;
        }




    }
}
