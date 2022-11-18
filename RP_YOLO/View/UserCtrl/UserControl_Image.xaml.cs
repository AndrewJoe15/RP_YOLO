using RP_YOLO.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

namespace RP_YOLO.View.UserCtrl
{
    /// <summary>
    /// UserControl_Image.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Image : UserControl
    {
        public UserControl_Image()
        {
            InitializeComponent();
            DataContext = new ROIViewModel();
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
            try
            {
                if (image == null)
                    return;
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Bmp);
                    SetImageSource(stream.ToArray());
                }
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        public void SetImageSource(byte[] buffer)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(buffer);
            bi.EndInit();
            img_image.Source = bi;
        }

    }
}
