using System.Collections.Generic;
using System.Drawing;

using Yolov5Net.Scorer;

namespace RP_YOLO.YOLO.Models
{
    internal class YoloV5FestoModel : YoloModel
    {
        public static int classCount = 2;

        public YoloV5FestoModel()
        {
            Dimensions = classCount + 5; // = 分类数 + 5

            Confidence = 0.20f;
            MulConfidence = 0.25f;
            Overlap = 0.45f;
            MaxDetections = 1;

            Labels = new List<YoloLabel>()
            {
                new YoloLabel { Id = 0, Name = "OK" , Color = Color.Green},
                new YoloLabel { Id = 1, Name = "NG" , Color = Color.Red}
            };
        }

    }
}
