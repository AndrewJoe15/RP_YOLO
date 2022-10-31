using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models.Abstract;

namespace RP_YOLO.Model
{
    /// <summary>
    /// YOLO 模型模板参数类
    /// </summary>
    public class YoloModelParam : INotifyPropertyChanged
    {
        public YoloModelParam(YoloModel yoloModel)
        {
            _yoloModel = yoloModel;
        }

        public YoloModel yoloModel
        {
            get => _yoloModel;
            set
            {
                if (yoloModel.Confidence != value.Confidence)
                {
                    _yoloModel.Confidence = value.Confidence;
                    OnPropertyChanged(nameof(yoloModel.Confidence));
                }
            }
        }

        private YoloModel _yoloModel;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
