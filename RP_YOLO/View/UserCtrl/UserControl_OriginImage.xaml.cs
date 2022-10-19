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

namespace RP_YOLO.View.UserCtrl
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_OriginImage : UserControl
    {
        public UserControl_OriginImage()
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
    }
}
