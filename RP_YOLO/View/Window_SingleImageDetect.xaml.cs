using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
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
using System.Windows.Forms;
using System.IO;

using Yolov5Net;
using Yolov5Net.Scorer;
using RP_YOLO.YOLO;
using RP_YOLO.YOLO.Models;
using System.Collections.ObjectModel;
using Microsoft.ML.OnnxRuntime;
using RP_YOLO.Model;

namespace RP_YOLO.View
{
    /// <summary>
    /// Window_SingleImageDetect.xaml 的交互逻辑
    /// </summary>
    public partial class Window_SingleImageDetect : Window
    {
        public ObservableCollection<DetectResult> detectResults;

        private string _sourceFolderName;//源文件夹
        private string _originImagePath { get => _sourceFolderName + "\\" + lsb_sourceFiles.SelectedItem; }//原始图片路径
        private bool _isRunning = false; //运行flag
        private YOLOV5<YoloV5SolderModel> yolov5;

        public Window_SingleImageDetect()
        {
            InitializeComponent();

            detectResults = new ObservableCollection<DetectResult>();
            dg_detectResult.DataContext = detectResults;
        }

        /// <summary>
        /// 源目录选择按钮点击
        /// 加载目录下的图片、视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void Btn_SourceDir_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Environment.CurrentDirectory;
            folderBrowserDialog.Description = "请选择源文件目录";

            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                lsb_sourceFiles.Items.Clear();

                _sourceFolderName = folderBrowserDialog.SelectedPath;
                DirectoryInfo directoryInfo = new DirectoryInfo(_sourceFolderName);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    lsb_sourceFiles.Items.Add(file.Name);
                }
            }
        }

        private void btn_browse_modelFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "onnx files(*.onnx)|*.onnx"};
            openFileDialog.Title = "请选择模型onnx文件";

            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string onnxPath = tbx_modelFile.Text = openFileDialog.FileName;
                yolov5 = new YOLOV5<YoloV5SolderModel>(onnxPath);
            }
        }

        private void lsb_sourceFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            uct_image.ShowImage(string.Empty);

            uct_image.ShowImage(_originImagePath);
            
            if (_isRunning)
                RunDetect();
            
        }

        private void btn_run_Click(object sender, RoutedEventArgs e)
        {
            if (tbx_modelFile.Text == null)
            {
                System.Windows.MessageBox.Show("请先选择onnx文件");
                return;
            }

            if (!_isRunning)
            {
                _isRunning = true;
                btn_run.Content = "停止";

                RunDetect();
            }
            else
            {
                _isRunning = false;
                btn_run.Content = "运行";
            }            
        }

        private void RunDetect()
        {
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(_originImagePath);

                yolov5.ObjectDetect(image, out DetectResult result);
                detectResults.Add(result);

                uct_image.ShowImage(image);

            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(System.IO.FileNotFoundException))
                {
                    System.Windows.MessageBox.Show("请选择源文件");
                }
            }
        }
    }
}
