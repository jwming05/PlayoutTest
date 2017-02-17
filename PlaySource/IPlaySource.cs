using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public interface IPlaySource
    {
        string Title { get; }

        PlayRange PlayRange { get; }

        IMediaSource MediaSource { get; }
    }

    public static class PlaySourceExtensions
    {
        //public static bool IsSegment(this IPlaySource playSource)
        //{
        //    return playSource is PlaySourceSegment;
        //}

        internal static PlayRange AdjustPlayRange(this IPlaySource playSource, PlayRange playRange)
        {
            var range = playSource.MediaSource.AdjustPlayRange(playRange);

            if(range.StartPosition<playSource.PlayRange.StartPosition ||
                range.StopPosition > playSource.PlayRange.StopPosition)
            {
                throw new ArgumentOutOfRangeException("playRange", 
                    string.Format("<{0}>无效，有效范围为{1}。",playRange,playSource.PlayRange));
            }

            return range;
        }
    }
}
