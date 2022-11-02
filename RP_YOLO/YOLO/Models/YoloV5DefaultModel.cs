using System.Collections.Generic;
using System.Drawing;

using Yolov5Net.Scorer;

namespace RP_YOLO.YOLO.Models
{
    internal class YoloV5DefaultModel : YoloModel
    {
        public YoloV5DefaultModel()
        {
            Confidence = 0.20f;
            MulConfidence = 0.25f;
            Overlap = 0.45f;
            MaxDetections = 30;

            Outputs = new[] { "output", "output0" };

            Labels = new List<YoloLabel>();
        }
    }
}
