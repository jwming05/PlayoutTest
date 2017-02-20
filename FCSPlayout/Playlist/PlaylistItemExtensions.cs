using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public static class PlaylistItemExtensions
    {
        //public static void Truncate(this IPlaylistItem schedulablePlayItem, DateTime maxStopTime)
        //{
        //    var scheduleInfo = schedulablePlayItem.ScheduleInfo;
        //    if (scheduleInfo.StopTime > maxStopTime)
        //    {
        //        TimeSpan duration = maxStopTime.Subtract(scheduleInfo.StartTime);
        //        duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;

        //        var startTime = duration == TimeSpan.Zero ? maxStopTime : scheduleInfo.StartTime;
        //        schedulablePlayItem.ScheduleInfo = new ScheduleInfo(startTime, duration, scheduleInfo.ScheduleMode);
        //    }
        //}

        public static bool IsSkipped(this IPlaylistItem playlistItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return playlistItem.PlaybillItem.IsSkipped();
        }

        public static bool IsSegment(this IPlaylistItem playlistItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return playlistItem.PlaybillItem.IsSegment;
        }

        public static bool IsFirstSegment(this IPlaylistItem playlistItem)
        {
            // Note: 定时播和定时插播总是抛出异常。
            return playlistItem.PlaybillItem.IsFirstSegment;
        }

        public static bool IsLastSegment(this IPlaylistItem playlistItem)
        {
            // Note: 定时播和定时插播总是抛出异常。
            return playlistItem.PlaybillItem.IsLastSegment;
        }

        public static bool IsTruncated(this IPlaylistItem playlistItem)
        {
            // Note: 定时播和定时插播应该总是返回false。
            return playlistItem.PlaybillItem.IsTruncated();
        }
    }
}
