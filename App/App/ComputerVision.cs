using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DuckHunt
{
    public class ComputerVision
    {
        const int MaxBackground = 3000;

        private BackgroundAggregator _background = new BackgroundAggregator(MaxBackground);

        const int AggregationBuffer = 50; //mm

        public CVResult HandleNewFramePair(DepthFrame depthFrame, RGBFrame rgbFrame)
        {
            CVResult result = null;

            DepthFrame background = _background.GetBackground(depthFrame);
            if (background != null)
            {
                result = new CVResult();
                Bitmap bmp = new System.Drawing.Bitmap(rgbFrame.Width, rgbFrame.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                byte[] buffer = new byte[bmd.Stride * bmd.Height];
                Marshal.Copy(bmd.Scan0, buffer, 0, buffer.Length);
                for (int y = 0, i = 0; y < depthFrame.Height; y++)
                {
                    for (int x = 0; x < depthFrame.Width; x++, i++)
                    {
                        short curDepth = depthFrame.RawData[i];
                        short backgroundDepth = background.RawData[i];
                        int index = (y * rgbFrame.Width + x) * 3; ;
                        int writeIndex = (y * rgbFrame.Width + x) * 3;


                        if (curDepth < MaxBackground && curDepth != 0 && (backgroundDepth == 0 || (backgroundDepth - curDepth > 150)))
                        {
                            byte r = rgbFrame.Data[index];
                            byte g = rgbFrame.Data[index + 1];
                            byte b = rgbFrame.Data[index + 2];

                            buffer[writeIndex] = b;
                            buffer[writeIndex + 1] = g;
                            buffer[writeIndex + 2] = r;
                        }
                        else
                        {
                            buffer[writeIndex] = 0;
                            buffer[writeIndex + 1] = 0;
                            buffer[writeIndex + 2] = 0;
                        }
                    }
                }
                Marshal.Copy(buffer, 0, bmd.Scan0, buffer.Length);
                bmp.UnlockBits(bmd);

                BlobCounter bc = new BlobCounter();
                bc.FilterBlobs = true;
                bc.MinWidth = 35;
                bc.MinHeight = 50;
                bc.ObjectsOrder = ObjectsOrder.Size;
                bc.ProcessImage(bmp);

                Blob[] blobs = bc.GetObjectsInformation();

                foreach (Blob blob in blobs)
                {
                    bc.ExtractBlobsImage(bmp, blob, true);
                }

                byte[] finalImage = new byte[1280 * 769 * 4];

                foreach (Blob blob in blobs)
                {
                    //float xRatio = blob.Rectangle.X / (rgbFrame.Width * 1.0f);
                    int newXPosition = blob.Rectangle.X + 480; // (int)Math.Round(xRatio * 1280);

                    float resizeRatio = 200 / (blob.Image.Height * 1.0f);

                    ResizeBicubic resize = new ResizeBicubic(200, (int)Math.Round(resizeRatio * blob.Image.Width));
                    UnmanagedImage resizedImage = resize.Apply(blob.Image);

                    for (int y = 0, y1 = 650 - resizedImage.Height; y < resizedImage.Height; y++, y1++)
                    {
                        for (int x = 0, x1 = newXPosition; x < resizedImage.Width; x++, x1++)
                        {
                            Color color = resizedImage.GetPixel(x, y);

                            int index = (y1 * 1280 + x1) * 4;

                            if(color.R == 0 && color.G == 0 && color.B == 0)
                            {
                                finalImage[index] = 0;
                                finalImage[index + 1] = 0;
                                finalImage[index + 2] = 0;
                                finalImage[index + 3] = 0;
                            }
                            else
                            {
                                finalImage[index] = color.B;
                                finalImage[index + 1] = color.G;
                                finalImage[index + 2] = color.R;
                                finalImage[index + 3] = 255;
                            }
                            
                        }
                    }
                }

                result.Foreground = finalImage; 
            }

            return result;
        }
    }
}
