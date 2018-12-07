using OpenNIWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DuckHunt
{
    public class DepthFrame
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public short[] RawData { get; private set; }

        public static DepthFrame FromVideoFrameRef(VideoFrameRef frame)
        {
            if (frame.SensorType == Device.SensorType.Depth)
            {
                DepthFrame newFrame = new DepthFrame();
                newFrame.Width = frame.FrameSize.Width;
                newFrame.Height = frame.FrameSize.Height;
                newFrame.RawData = new short[frame.DataSize / sizeof(short)];
                Marshal.Copy(frame.Data, newFrame.RawData, 0, newFrame.RawData.Length);

                return newFrame;
            }
            else
            {
                throw new ArgumentException("frame must be from a sensor of type Depth");
            }
        }

        public static DepthFrame FromNumbers(int width, int height)
        {
            return new DepthFrame { Width = width, Height = height, RawData = new short[width * height] };
        }
    }
}
