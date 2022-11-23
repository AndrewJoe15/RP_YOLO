﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.ML.OnnxRuntime;
using RP_YOLO.Model;
using RPSoft_Core.Utils;
using Yolov5Net.Scorer;

namespace RP_YOLO.YOLO
{
    /// <summary>
    /// YOLOV5 封装类
    /// </summary>
    internal class YOLOV5
    {
        public YoloScorer scorer { get; set; }


        /// <summary>
        /// 指定模型参数和模型文件以构造对象
        /// </summary>
        /// <param name="yoloModel"></param>
        /// <param name="onnxPath"></param>
        public YOLOV5(YoloModel yoloModel, string onnxPath)
        {
            // 使用CUDA
            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.AppendExecutionProvider_CUDA();
            // 加载模型文件
            scorer = new YoloScorer(yoloModel, onnxPath, sessionOptions);
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
            List<YoloPrediction> predictions = scorer.Predict(image);
            stopwatch.Stop();
            result.during = stopwatch.ElapsedMilliseconds;

            Graphics graphics = Graphics.FromImage(image);

            // 遍历预测结果，画出预测框
            foreach (var prediction in predictions)
            {
                double score = Math.Round(prediction.Score, 2);

                graphics.DrawRectangles(new Pen(prediction.Label.Color, 4), new[] { prediction.Rectangle });

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

        public void ObjectDetect(System.Drawing.Image image, Rectangle rectangle, out DetectResult result)
        {
            result = new DetectResult();

            Stopwatch stopwatch = new Stopwatch();//计时器用来计算目标检测算法执行时间
            stopwatch.Start();
            List<YoloPrediction> predictions = scorer.Predict(image);
            stopwatch.Stop();
            result.during = stopwatch.ElapsedMilliseconds;

            var graphics = Graphics.FromImage(image);

            // 遍历预测结果，画出预测框
            foreach (var prediction in predictions)
            {
                double score = Math.Round(prediction.Score, 2);

                graphics.DrawRectangles(new Pen(prediction.Label.Color, 4), new[] { prediction.Rectangle });

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
