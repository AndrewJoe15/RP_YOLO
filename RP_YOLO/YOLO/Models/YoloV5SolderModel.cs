using System.Collections.Generic;
using System.Drawing;

using Yolov5Net.Scorer;

namespace RP_YOLO.YOLO.Models
{
    internal class YoloV5SolderModel : YoloModel
    {
        public static int classCount = 1;

        public YoloV5SolderModel()
        {
            Dimensions = classCount + 5; // = 分类数 + 5

            Confidence = 0.20f;
            MulConfidence = 0.25f;
            Overlap = 0.45f;
            MaxDetections = 20;

            Outputs = new[] { "output" };

            Labels = new List<YoloLabel>()
            {
                new YoloLabel { Id = 0, Name = "Bolt" , Color = Color.Red}
            };
        }
    }
}
