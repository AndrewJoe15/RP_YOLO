using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Microsoft.ML.OnnxRuntime;
using RP_YOLO.Model;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models.Abstract;

namespace RP_YOLO.YOLO
{
    /// <summary>
    /// YOLOV5 封装类
    /// </summary>
    class YOLOV5<T> where T : YoloModel
    {
        private YoloScorer<T> m_scorer;

        public YOLOV5(string onnxPath)
        {
            //使用CUDA
            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.AppendExecutionProvider_CUDA();
            //加载模型文件
            m_scorer = new YoloScorer<T>(onnxPath, sessionOptions);
        }

        /// <summary>
        /// 目标检测
        /// </summary>
        /// <param name="image">输出图片</param>
        /// <param name="quantity">数组 依次存放各个种类的数量</param>
        /// <param name="during">检测所用时间</param>
        public void ObjectDetect(System.Drawing.Image image, out DetectResult result)
        {
            result = new DetectResult();

            Stopwatch stopwatch = new Stopwatch();//计时器用来计算目标检测算法执行时间
            stopwatch.Start();
            List<YoloPrediction> predictions = m_scorer.Predict(image);
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
