using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public static class PlaybillItemExtensions
    {
        public static bool IsAuto(this PlaybillItem playbillItem)
        {
            return playbillItem.ScheduleMode == ScheduleMode.Auto;
        }

        public static bool IsTiming(this PlaybillItem playbillItem)
        {
            return playbillItem.ScheduleMode == ScheduleMode.Timing;
        }

        public static bool IsTimingBreak(this PlaybillItem playbillItem)
        {
            return playbillItem.ScheduleMode == ScheduleMode.TimingBreak;
        }

        public static bool IsAutoPadding(this PlaybillItem playbillItem)
        {
            return playbillItem.ScheduleMode == ScheduleMode.Auto;
        }

        public static bool IsSkipped(this PlaybillItem playbillItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return playbillItem.ScheduleInfo.Duration == TimeSpan.Zero;
        }

        public static bool IsTruncated(this PlaybillItem playbillItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return playbillItem.ScheduleInfo.Duration < playbillItem.PlaySource.PlayRange.Duration;
        }
    }
}
