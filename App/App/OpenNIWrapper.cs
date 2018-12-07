using OpenNIWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckHunt
{
    public class OpenNIWrapper
    {
        private Device _device;
        private Task _playTask;
        private VideoStream _depthStream, _colorStream;
        private bool _play = false; 

        private OpenNIWrapper()
        {

        }

        private void InnerStart()
        {
            OpenNI.Status status = OpenNI.Initialize();

            if (status == OpenNI.Status.Ok)
            {
                _device = Device.Open(null);

                _depthStream = _device.CreateVideoStream(Device.SensorType.Depth);
                _colorStream = _device.CreateVideoStream(Device.SensorType.Color);

                

                _playTask = Task.Factory.StartNew(() =>
                {
                    _device.DepthColorSyncEnabled = true; 
                    _depthStream.Start();
                    _colorStream.Start();
                    _device.ImageRegistration = Device.ImageRegistrationMode.DepthToColor; 

                    VideoStream readyStream = null;
                    _play = true; 
                    while (OpenNI.WaitForAnyStream(new VideoStream[] { _depthStream, _colorStream }, out readyStream) == OpenNI.Status.Ok && _play)
                    {
                        DepthFrame depthFrame = DepthFrame.FromVideoFrameRef(_depthStream.ReadFrame());
                        RGBFrame colorFrame = RGBFrame.FromVideoFrameRef(_colorStream.ReadFrame());

                        OnNewFrames?.Invoke(depthFrame, colorFrame); 
                    }
                });
            }
        }

        public static OpenNIWrapper Start()
        {
            OpenNIWrapper wrapper = new OpenNIWrapper();
            wrapper.InnerStart();

            return wrapper;
        }

        public void Stop()
        {
            _play = false;
            _playTask.Wait();

            OpenNI.Shutdown(); 
        }

        public event Action<DepthFrame, RGBFrame> OnNewFrames;
    }
}
