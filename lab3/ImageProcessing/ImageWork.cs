using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class ImageWork
    {
        public static void Processing(FileInfo file, List<TaskDetails> bag, int processedCount)
        {
            Stopwatch sw = Stopwatch.StartNew();
            double ms1 = sw.Elapsed.TotalMilliseconds;

            HeavyImageProcessing(file);

            string idx = file.Name;

            double ms2 = sw.Elapsed.TotalMilliseconds;
            int thr = Thread.CurrentThread.ManagedThreadId;
            int task = Task.CurrentId.Value;
            bag.Add(new TaskDetails(thr, task, idx, ms1, ms2));
            Interlocked.Increment(ref processedCount);
        }

        private static void HeavyImageProcessing(FileInfo file)
        {
            Bitmap originalImage = new Bitmap(file.FullName);
            Bitmap adjustedImage = new Bitmap(originalImage);
            float brightness = 1.0f; // no change in brightness
            float contrast = 2.0f; // twice the contrast
            float gamma = 1.0f; // no change in gamma

            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
                new float[] {contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            Graphics g = Graphics.FromImage(adjustedImage);
            g.DrawImage(originalImage, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
                , 0, 0, originalImage.Width, originalImage.Height,
                GraphicsUnit.Pixel, imageAttributes);

            adjustedImage.Save(file.Directory + @"\out\" + file.Name);
        }

        private static void TheHeaviestImageProcessing(FileInfo file)
        {
            //read image
            Bitmap bmp = new Bitmap(file.FullName);

            //get image dimension
            int width = bmp.Width;
            int height = bmp.Height;

            //color of pixel
            Color p;

            //sepia
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //get pixel value
                    p = bmp.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //calculate temp value
                    int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                    int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                    int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);

                    //set new RGB value
                    if (tr > 255)
                    {
                        r = 255;
                    }
                    else
                    {
                        r = tr;
                    }

                    if (tg > 255)
                    {
                        g = 255;
                    }
                    else
                    {
                        g = tg;
                    }

                    if (tb > 255)
                    {
                        b = 255;
                    }
                    else
                    {
                        b = tb;
                    }

                    //set the new RGB value in image pixel
                    bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            //save (write) sepia image
            bmp.Save(file.Directory + @"\out\" + file.Name);        }
    }
}
