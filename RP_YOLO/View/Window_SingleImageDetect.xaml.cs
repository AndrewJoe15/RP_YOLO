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
        private string _onnxPath; //onnx文件路径
        private bool _isRunning = false; //运行flag
        private YoloScorer<YoloV5SolderModel> _scorer;

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
                _onnxPath = tbx_modelFile.Text = openFileDialog.FileName;

                //使用CUDA
                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.AppendExecutionProvider_CUDA();
                //加载模型文件
                _scorer = new YoloScorer<YoloV5SolderModel>(_onnxPath, sessionOptions);
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
            if (_onnxPath == null)
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

                ObjectDetect(image, out DetectResult result);
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

        /// <summary>
        /// 目标检测
        /// </summary>
        /// <param name="image">输出图片</param>
        /// <param name="quantity">数组 依次存放各个种类的数量</param>
        /// <param name="during">检测所用时间</param>
        private void ObjectDetect(System.Drawing.Image image, out DetectResult result)
        {
            result = new DetectResult();

            Stopwatch stopwatch = new Stopwatch();//计时器用来计算目标检测算法执行时间
            stopwatch.Start();
            List<YoloPrediction> predictions = _scorer.Predict(image);
            stopwatch.Stop();
            result.during = stopwatch.ElapsedMilliseconds;

            var graphics = Graphics.FromImage(image);

            // 遍历预测结果，画出预测框
            foreach (var prediction in predictions)
            {
                double score = Math.Round(prediction.Score, 2);

                graphics.DrawRectangles(new System.Drawing.Pen(prediction.Label.Color, 2), new[] { prediction.Rectangle });

                var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

                graphics.DrawString($"{prediction.Label.Name} ({score})", 
                    new Font("Consolas", 24, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color), new PointF(x, y));

                switch (prediction.Label.Id) 
                {
                    case 0:
                        result.OK++;
                        break;
                    case 1:
                        result.NG++;
                        break;
                }
            }
        }
    }
}
