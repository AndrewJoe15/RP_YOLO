using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RP_YOLO.ViewModel
{
    class ROIViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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

        private Point bottomRightAnchor = new Point(64, 48);
        public Point BottomRightAnchor
        {
            get { return bottomRightAnchor; }
            set
            {
                bottomRightAnchor = value;
                OnPropertyChanged();
            }
        }
    }
}
