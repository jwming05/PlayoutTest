using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public static class MediaSourceExtensions
    {
        public static bool HasNativeDuration(this IMediaSource mediaSource)
        {
            return mediaSource.NativeDuration != null;
        }

        public static PlayRange Adjust(this IMediaSource mediaSource, PlayRange playRange)
        {
            if (!mediaSource.HasNativeDuration())
            {
                playRange = new PlayRange(playRange.Duration);
            }
            mediaSource.ValidatePlayRange(playRange);
            return playRange;
        }

        private static bool IsValidPlayRange(this IMediaSource mediaSource, PlayRange playRange)
        {
            if (mediaSource.HasNativeDuration())
            {
                return mediaSource.GetNativePlayRange().Include(playRange);
            }
            else
            {
                return playRange.StartPosition == TimeSpan.Zero;
            }
        }

        private static void ValidatePlayRange(this IMediaSource mediaSource, PlayRange playRange)
        {
            if (!mediaSource.IsValidPlayRange(playRange))
            {
                if (mediaSource.HasNativeDuration())
                {
                    throw new ArgumentException(
                        string.Format("{0}无效，有效范围为：{1}。", playRange, mediaSource.GetNativePlayRange()),
                        "playRange");
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("{0}无效，起始位置必须为TimeSpan.Zero。", playRange),
                        "playRange");
                }
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

        internal static bool CanMerge(this IMediaSource mediaSource, PlayRange first, PlayRange second)
        {
            return mediaSource.HasNativeDuration() ? 
                PlayRange.IsUnbroken(mediaSource.Adjust(first), mediaSource.Adjust(second)) : true;
        }

        internal static PlayRange Merge(this IMediaSource mediaSource, PlayRange first, PlayRange second)
        {
            if (!mediaSource.CanMerge(first, second))
            {
                throw new InvalidOperationException();
            }

            first = mediaSource.Adjust(first);
            second = mediaSource.Adjust(second);

            if (mediaSource.HasNativeDuration())
            {
                return PlayRange.Merge(first, second);
            }
            else
            {
                return new PlayRange(first.Duration + second.Duration);
            }
        }

        internal static bool IsAutoPadding(this IMediaSource mediaSource)
        {
            return mediaSource is AutoPaddingMediaSource;
        }
    }
}
