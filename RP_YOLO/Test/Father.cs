using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RP_YOLO.Test
{
    internal abstract class Father : INotifyPropertyChanged
    {
        public virtual int Age {
            get => Age;
            set
            {
                if (Age != value)
                {
                    Age = value;
                    OnPropertyChanged(nameof(Age));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
