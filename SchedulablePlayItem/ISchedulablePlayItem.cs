using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public interface ISchedulablePlayItem
    {
        string Title { get; }

        ScheduleInfo ScheduleInfo { get; set; }

        IPlaySource PlaySource { get; }
    }

    public static class SchedulablePlayItemExtensions
    {
        public static bool IsSegment(this ISchedulablePlayItem schedulablePlayItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return PlaySource.IsSegment(schedulablePlayItem.PlaySource);
        }

        public static bool IsSkipped(this ISchedulablePlayItem schedulablePlayItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return schedulablePlayItem.ScheduleInfo.Duration == TimeSpan.Zero;
        }

        public static bool IsTruncated(this ISchedulablePlayItem schedulablePlayItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return schedulablePlayItem.ScheduleInfo.Duration < schedulablePlayItem.PlaySource.PlayRange.Duration;
        }

        public static void Truncate(this ISchedulablePlayItem schedulablePlayItem, DateTime maxStopTime)
        {
            var scheduleInfo = schedulablePlayItem.ScheduleInfo;
            if (scheduleInfo.StopTime > maxStopTime)
            {
                TimeSpan duration = maxStopTime.Subtract(scheduleInfo.StartTime); 
                duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;

                var startTime = duration == TimeSpan.Zero ? maxStopTime : scheduleInfo.StartTime;
                schedulablePlayItem.ScheduleInfo = new ScheduleInfo(startTime,duration,scheduleInfo.ScheduleMode);
            }
        }
    }
}
