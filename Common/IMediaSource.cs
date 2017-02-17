using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public interface IMediaSource
    {
        string Title { get; }
        TimeSpan? NativeDuration { get; }
    }

    public static class MediaSourceExtensions
    {
        public static bool HasNativeDuration(this IMediaSource mediaSource)
        {
            return mediaSource.NativeDuration != null;
        }

        public static PlayRange AdjustPlayRange(this IMediaSource mediaSource, PlayRange playRange)
        {
            mediaSource.ValidatePlayRange(playRange);
            return mediaSource.HasNativeDuration() ? playRange : new PlayRange(playRange.Duration);
        }

        public static bool IsValidPlayRange(this IMediaSource mediaSource, PlayRange playRange)
        {
            return !mediaSource.HasNativeDuration() || 
                (playRange.StartPosition >= TimeSpan.Zero && playRange.StopPosition <= mediaSource.NativeDuration.Value);
        }

        public static void ValidatePlayRange(this IMediaSource mediaSource, PlayRange playRange)
        {
            if (mediaSource.HasNativeDuration() && !mediaSource.IsValidPlayRange(playRange))
            {
                throw new ArgumentException(
                        string.Format("{0}无效，有效范围为：{1}。", playRange, mediaSource.GetNativePlayRange()),
                        "playRange");
            }
        }

        public static PlayRange GetNativePlayRange(this IMediaSource mediaSource)
        {
            if (!mediaSource.HasNativeDuration())
            {
                throw new ArgumentException("没有原生时长。", "mediaSource");
            }

            return new PlayRange(mediaSource.NativeDuration.Value);
        }

        internal static bool CanMerge(this IMediaSource mediaSource, PlayRange range1, PlayRange range2)
        {
            range1 = mediaSource.AdjustPlayRange(range1);
            range2 = mediaSource.AdjustPlayRange(range2);

            return mediaSource.HasNativeDuration() ? PlayRange.IsUnbroken(range1, range2) : true;
        }

        internal static PlayRange Merge(this IMediaSource mediaSource, PlayRange range1, PlayRange range2)
        {
            if (!mediaSource.CanMerge(range1, range2))
            {
                throw new InvalidOperationException();
            }

            range1 = mediaSource.AdjustPlayRange(range1);
            range2 = mediaSource.AdjustPlayRange(range2);

            if (mediaSource.HasNativeDuration())
            {
                return PlayRange.Merge(range1,range2);
            }
            else
            {
                return new PlayRange(range1.Duration + range2.Duration);
            }
        }
    }
}
