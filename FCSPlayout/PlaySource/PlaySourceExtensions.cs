using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public static class PlaySourceExtensions
    {
        internal static PlayRange Adjust(this IPlaySource playSource, PlayRange playRange)
        {
            var range = playSource.MediaSource.Adjust(playRange);

            if (!playSource.PlayRange.Include(range))
            {
                throw new ArgumentOutOfRangeException("playRange",
                    string.Format("<{0}>无效，有效范围为{1}。", playRange, playSource.PlayRange));
            }

            return range;
        }

        internal static bool IsAutoPadding(this IPlaySource playSource)
        {
            return playSource.MediaSource.IsAutoPadding();
        }

        internal static void Break(this PlaySource playSource, TimeSpan duration, out PlayRange first, out PlayRange second)
        {
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("duration",
                    string.Format("<{0}>无效，必须大于TimeSpan.Zero。", duration));
            }
            if (duration >= playSource.PlayRange.Duration)
            {
                throw new ArgumentOutOfRangeException("duration",
                    string.Format("<{0}>无效，必须小于{1}。", duration, playSource.PlayRange.Duration));
            }

            PlayRange.Break(playSource.PlayRange, duration, out first, out second);

            first = playSource.Adjust(first);
            second = playSource.Adjust(second);
        }
    }
}
