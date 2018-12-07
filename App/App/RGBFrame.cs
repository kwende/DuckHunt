using OpenNIWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DuckHunt
{
    public class RGBFrame
    {
        public int Height { get; private set; }
        public int Width { get; private set; }
        public byte[] Data { get; private set; }

        public static RGBFrame FromVideoFrameRef(VideoFrameRef frame)
        {
            if(frame.SensorType == Device.SensorType.Color)
            {
                RGBFrame newFrame = new RGBFrame();
                newFrame.Width = frame.FrameSize.Width;
                newFrame.Height = frame.FrameSize.Height;
                newFrame.Data = new byte[frame.DataSize];

                Marshal.Copy(frame.Data, newFrame.Data, 0, newFrame.Data.Length);

                return newFrame; 
            }
            else
            {
                throw new ArgumentException("frame must be from a sensor of type Color."); 
            }
        }
    }
}
