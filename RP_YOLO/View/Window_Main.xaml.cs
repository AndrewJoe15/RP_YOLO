using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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

using RP_YOLO.View;

namespace RP_YOLO
{
    /// <summary>
    /// Window_Main.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Main : Window
    {
        public Window_Main()
        {
            InitializeComponent();
        }

        private void btn_YOLOV5_SingleImage_Click(object sender, RoutedEventArgs e)
        {
            Window_SingleImageDetect window_SingleImageDetect = new Window_SingleImageDetect();
            window_SingleImageDetect.Owner = this;
            window_SingleImageDetect.Show();
        }

        private void btn_YOLOV5_camera_Click(object sender, RoutedEventArgs e)
        {
            Window_CameraStreamDetect window_CameraStreamDetect = new Window_CameraStreamDetect();
            window_CameraStreamDetect.Owner = this;
            window_CameraStreamDetect.Show();
        }
    }
}
