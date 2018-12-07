using OpenNIWrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DuckHunt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenNIWrapper _wrapper;
        private ComputerVision _cv;
        private WriteableBitmap _bmp;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _cv = new ComputerVision();
            _wrapper = OpenNIWrapper.Start();
            _wrapper.OnNewFrames += _wrapper_OnNewFrames;
        }

        private void _wrapper_OnNewFrames(DepthFrame depthFrame, RGBFrame rgbFrame)
        {
            CVResult result = _cv.HandleNewFramePair(depthFrame, rgbFrame);

            if (_bmp == null)
            {
                Dispatcher.Invoke(() =>
                {
                    _bmp = new WriteableBitmap(1280, 769, 96, 96, PixelFormats.Bgra32, null);
                    VideoWindow.Source = _bmp;
                });
            }

            if (result != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _bmp.WritePixels(new Int32Rect(0, 0, 1280, 769),
                        result.Foreground, 1280 * 4, 0);
                });
            }

        }

        private void EscapeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _wrapper.Stop();
            this.Close();
        }
    }
}
