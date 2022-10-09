using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP_YOLO.Model
{
    /// <summary>
    /// 检测结果类
    /// 记录检测出的目标种类个数、检测时间、帧率
    /// </summary>
    public class DetectResult
    {        
        public int OK { get; set; }
        public int NG { get; set; }
        public long during { get; set; }
        public long FPS { get { return during == 0 ? 0 : 1000 / during; } set { } }
    }
}
