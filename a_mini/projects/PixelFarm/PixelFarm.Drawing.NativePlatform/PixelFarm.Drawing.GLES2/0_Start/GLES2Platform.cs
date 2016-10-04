﻿//BSD, 2014-2016, WinterDev

using System;
using System.IO;
using System.Collections.Generic;
using PixelFarm.Drawing.Fonts;
using PixelFarm.DrawingGL;
using Microsoft.Win32;
using System.Text;
namespace PixelFarm.Drawing.GLES2
{

    class GLES2Platform : GraphicsPlatform
    {

        FontSystem fontSystem = new FontSystem();
        public GLES2Platform()
        {
        }

        public override IFonts SampleIFonts
        {
            get
            {
                return fontSystem;
            }
        }
        public override Canvas CreateCanvas(int left, int top, int width, int height)
        {
            int max = Math.Max(width, height);
            CanvasGL2d canvas2d = new CanvasGL2d(max, max);
            MyGLCanvas myCanvas = new MyGLCanvas(this, canvas2d, 0, 0, width, height);
            return myCanvas;
        }
        public override Canvas CreateCanvas(object platformCanvas, int left, int top, int width, int height)
        {
            int max = Math.Max(width, height);
            CanvasGL2d canvas2d = new CanvasGL2d(max, max);
            MyGLCanvas myCanvas = new MyGLCanvas(this, canvas2d, 0, 0, width, height);
            return myCanvas;
        }

        public override GraphicsPath CreateGraphicsPath()
        {
            return new GLES2GraphicsPath();
        }
        public override Bitmap CreatePlatformBitmap(int w, int h, byte[] rawBuffer, bool isBottomUp)
        {
            GLESBitmap innerBmp = new GLESBitmap(w, h);
            innerBmp.Buffer = rawBuffer;
            innerBmp.IsBottomUp = isBottomUp;

            Bitmap bmp = new Bitmap(w, h, innerBmp);
            return bmp;
        }
        public override Font GetFont(string fontfaceName, float emsize, FontStyle st)
        {   
            return fontSystem.GetFont(fontfaceName, emsize, st);
        }
        public override ActualFont GetActualFont(Font f)
        {
            return fontSystem.fontStore.GetResolvedNativeFont(f);
        }

    }
}