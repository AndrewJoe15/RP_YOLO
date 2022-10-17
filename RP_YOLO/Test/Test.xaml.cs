using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RP_YOLO.Test
{
    /// <summary>
    /// Test.xaml 的交互逻辑
    /// </summary>
    public partial class Test : Window
    {
        public Test()
        {
            InitializeComponent();
        }



        private void ThreadProc()
        {

            _ = Dispatcher.InvokeAsync(new Action(delegate
            {
                lb_thread.Content += "1";
            }));

            _ = Dispatcher.InvokeAsync(() => { lb_thread.Content += "2"; }, System.Windows.Threading.DispatcherPriority.Render);

            _ = Dispatcher.InvokeAsync(() => { lb_thread.Content += "3"; }, System.Windows.Threading.DispatcherPriority.Render);
        }

        private void btn_thread_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(ThreadProc);
            thread.Start();

            lb_thread.Content = "0";
        }
    }
}
