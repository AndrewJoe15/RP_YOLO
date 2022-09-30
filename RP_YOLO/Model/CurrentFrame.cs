using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RPSoft_Core.Objs;

namespace RP_YOLO.Model
{
    class CurrentFrame
    {
        public static UInt32 nBufSizeForDriver;
        public static IntPtr bufForDriver = IntPtr.Zero;
        public static System.Drawing.Bitmap bitmap { get; set; }
    }
}
