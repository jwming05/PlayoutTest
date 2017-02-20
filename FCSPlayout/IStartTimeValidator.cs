using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    /// <summary>
    /// 表示定时播或定时插播开始时间验证的接口。
    /// </summary>
    public interface ITimingScheduleValidator
    {
        /// <summary>
        /// 验证定时播或定时插播开始时间是否有效。
        /// </summary>
        /// <param name="startTime">指定定时播或定时插播开始时间。</param>
        /// <param name="isBreak">
        /// 指定<paramref name="startTime"/>是定时播还是定时插播的开始时间。<c>true</c>表示是定时插播，<c>false</c>表示定时播。
        /// </param>
        /// <returns><paramref name="startTime"/>有效则返回<c>true</c>，否则返回<c>false</c>。</returns>
        bool IsValid(DateTime startTime, bool isBreak);
    }
}
