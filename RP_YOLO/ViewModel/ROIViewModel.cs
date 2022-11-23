using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RP_YOLO.ViewModel
{
    public class ROIViewModel : INotifyPropertyChanged
    {
        public ROIViewModel(Point anchor_tl, Point anchor_br)
        {
            topLeftAnchor = anchor_tl;
            bottomRightAnchor = anchor_br;
            roiWidth = (anchor_br - anchor_tl).X;
            roiHeight = (anchor_br - anchor_tl).Y;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool isVisible = false;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }

        private bool isFixed = false;
        public bool IsFixed
        {
            get => isFixed;
            set
            {
                isFixed = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 左上锚点
        /// </summary>
        private Point topLeftAnchor = new Point(0, 0);
        public Point TopLeftAnchor
        {
            get { return topLeftAnchor; }
            set
            {
                topLeftAnchor = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 右下锚点
        /// </summary>
        private Point bottomRightAnchor = new Point(640, 640);
        public Point BottomRightAnchor
        {
            get { return bottomRightAnchor; }
            set
            {
                bottomRightAnchor = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 中心点
        /// </summary>
        private Point center;
        public Point Center
        {
            get => center;
            set
            {
                center = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// ROI框宽
        /// </summary>
        private double roiWidth;
        public double RoiWidth
        {
            get => roiWidth;
            set
            {
                roiWidth = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// ROI框高
        /// </summary>
        public double roiHeight;
        public double RoiHeight
        {
            get => roiHeight;
            set
            {
                roiHeight = value;
                OnPropertyChanged();
            }
        } 
    }
}
