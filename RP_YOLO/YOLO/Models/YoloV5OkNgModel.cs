using System.Collections.Generic;
using System.Drawing;

using Yolov5Net.Scorer;

namespace RP_YOLO.YOLO.Models
{
    internal class YoloV5OkNgModel : YoloModel
    {
        public YoloV5OkNgModel()
        {
            Confidence = 0.20f;
            MulConfidence = 0.25f;
            Overlap = 0.45f;
            MaxDetections = 10;

            Labels = new List<YoloLabel>()
            {
                new YoloLabel { Id = 0, Name = "OK" , Color = YoloLabel.Colors[0]},
                new YoloLabel { Id = 1, Name = "NG" , Color = YoloLabel.Colors[1]}
            };
        }
    }
}
