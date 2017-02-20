using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public static class PlaylistExtensions
    {
        public static bool HasMinStartTime(this IPlaylist playlist)
        {
            return playlist.MinStartTime != null;
        }

        public static int FindLastIndex(this IPlaylist playlist, Func<IPlaylistItem, bool> predicate)
        {
            return playlist.FindLastIndex(playlist.Count - 1, predicate);
        }

        public static int FindLastIndex(this IPlaylist playlist, int startLastIndex, Func<IPlaylistItem, bool> predicate)
        {
            if (startLastIndex >= playlist.Count)
            {
                throw new ArgumentOutOfRangeException("startLastIndex");
            }

            for (int i = startLastIndex; i >= 0; i--)
            {
                if (predicate(playlist[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int FindFirstIndex(this IPlaylist playlist, Func<IPlaylistItem, bool> predicate)
        {
            return playlist.FindFirstIndex(0, predicate);
        }

        public static int FindFirstIndex(this IPlaylist playlist, int startFirstIndex, Func<IPlaylistItem, bool> predicate)
        {
            if (startFirstIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startFirstIndex");
            }

            for (int i = startFirstIndex; i < playlist.Count; i++)
            {
                if (predicate(playlist[i]))
                {
                    return i;
                }
            }
            return -1;
        }


        internal static void EnsureNoTimingConflict(this IPlaylist playlist, DateTime startTime, TimeSpan duration)
        {
            var stopTime = startTime.Add(duration);

            var index = playlist.FindFirstIndex((i) =>
                (i.ScheduleInfo.ScheduleMode == ScheduleMode.Timing || i.ScheduleInfo.ScheduleMode == ScheduleMode.TimingBreak) &&
                !i.IsSkipped() &&
                (i.ScheduleInfo.StopTime > startTime && i.ScheduleInfo.StartTime < stopTime)); //!(i.ScheduleInfo.StopTime<=startTime || i.ScheduleInfo.StartTime>=stopTime)

            if (index >= 0)
            {
                var item = playlist[index];
                throw new InvalidOperationException(string.Format("与{0}'{1}'之间有时间冲突。",
                    item.ScheduleInfo.ScheduleMode == ScheduleMode.Timing ? "定时播" : "定时插播", item.Title));
            }
        }
    }
}
