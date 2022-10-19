using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

namespace RP_YOLO.View.UserCtrl
{
    /// <summary>
    /// UserControl_DetectedImage.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_DetectedImage : UserControl
    {
        public UserControl_DetectedImage()
        {
            InitializeComponent();
        }

        public void ShowImage(string path)
        {
            if (path == string.Empty)
            {
                img_image.Source = null;
                return;
            }

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(path);
            bitmapImage.EndInit();

            img_image.Source = bitmapImage;

            txb_fileName.Text = path.Substring(path.LastIndexOf('\\'));

        }

        public void ShowImage(System.Drawing.Image image)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray()); // 不要直接使用 ms
            bi.EndInit();
            img_image.Source = bi;
            ms.Close();
        }
    }
}
