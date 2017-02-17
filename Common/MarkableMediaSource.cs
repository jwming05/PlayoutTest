using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public struct MarkableMediaSource
    {
        public static void Validate(MarkableMediaSource markableMediaSource)
        {
            if (markableMediaSource.MediaSource == null)
            {
                throw new ArgumentException("未初始化。", "markableMediaSource");
            }
        }

        public static void Validate(MarkableMediaSource markableMediaSource, TimeSpan minDuration)
        {
            if (markableMediaSource.MediaSource == null)
            {
                throw new ArgumentException("未初始化。", "markableMediaSource");
            }

            if (minDuration < TimeSpan.Zero)
            {
                throw new ArgumentException(string.Format("<{0}>无效，用于验证的最小时间不能小于TimeSpan.Zero。", minDuration), 
                    "minDuration");
            }

            if (markableMediaSource.PlayRange.Duration < minDuration)
            {
                throw new ArgumentException(
                    string.Format("<{0}>无效，时长不能小于{1}。", markableMediaSource.PlayRange.Duration, minDuration), 
                    "markableMediaSource");
            }
        }

        public IMediaSource MediaSource { get; private set; }
        public PlayRange PlayRange { get; private set; }

        public MarkableMediaSource(IMediaSource mediaSource, PlayRange playRange)
        {
            if (mediaSource == null)
            {
                throw new ArgumentNullException("mediaSource");
            }


            var adjustedPlayRange = mediaSource.AdjustPlayRange(playRange); // 包含对playRange有效性的验证


            this.MediaSource = mediaSource;
            this.PlayRange = adjustedPlayRange;
        }

        public MarkableMediaSource(IMediaSource mediaSource)
        {
            if (mediaSource == null)
            {
                throw new ArgumentNullException("mediaSource");
            }

            this.PlayRange = mediaSource.GetNativePlayRange();            
            this.MediaSource = mediaSource;
        }
    }
}
