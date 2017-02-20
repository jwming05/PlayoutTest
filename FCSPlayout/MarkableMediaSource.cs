using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public sealed class MarkableMediaSource
    {
        //public static void Validate(MarkableMediaSource markableMediaSource)
        //{
        //    if (markableMediaSource.MediaSource == null)
        //    {
        //        throw new ArgumentException("未初始化。", "markableMediaSource");
        //    }
        //}

        //public static void Validate(MarkableMediaSource markableMediaSource, TimeSpan minDuration)
        //{
        //    if (markableMediaSource.MediaSource == null)
        //    {
        //        throw new ArgumentException("未初始化。", "markableMediaSource");
        //    }

        //    if (minDuration < TimeSpan.Zero)
        //    {
        //        throw new ArgumentException(string.Format("<{0}>无效，用于验证的最小时间不能小于TimeSpan.Zero。", minDuration), 
        //            "minDuration");
        //    }

        //    if (markableMediaSource.PlayRange.Duration < minDuration)
        //    {
        //        throw new ArgumentException(
        //            string.Format("<{0}>无效，时长不能小于{1}。", markableMediaSource.PlayRange.Duration, minDuration), 
        //            "markableMediaSource");
        //    }
        //}

        

        public IMediaSource MediaSource { get; private set; }
        public PlayRange PlayRange { get; private set; }

        public string Title
        {
            get
            {
                return this.MediaSource.Title;
            }
        }

        //public TimeSpan? NativeDuration
        //{
        //    get
        //    {
        //        return this.MediaSource.NativeDuration;
        //    }
        //}

        public MarkableMediaSource(IMediaSource mediaSource, PlayRange playRange)
        {
            if (mediaSource == null)
            {
                throw new ArgumentNullException("mediaSource");
            }


            var adjustedPlayRange = mediaSource.Adjust(playRange); // 包含对playRange有效性的验证

            //MarkableMediaSource markable = mediaSource as MarkableMediaSource;
            //if (markable!=null)
            //{
            //    this.MediaSource = markable.MediaSource;
            //}
            //else
            //{
            //    this.MediaSource = mediaSource;
            //}

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

        
        internal PlayRange Adjust(PlayRange range)
        {
            range = this.MediaSource.Adjust(range);
            if (!this.PlayRange.Include(range))
            {
                throw new ArgumentOutOfRangeException("range",
                    string.Format("<{0}>无效，有效范围为{1}。", range, this.PlayRange));
            }

            return range;
        }

        //internal bool CanMerge(PlayRange first,PlayRange second)
        //{
        //    return this.MediaSource.CanMerge(Adjust(first), Adjust(second));
        //}

        //internal PlayRange Merge(PlayRange first, PlayRange second)
        //{
        //    var playRange = this.MediaSource.Merge(Adjust(first), Adjust(second));
        //    return Adjust(playRange); // 合并以后进行验证。
        //}
    }
}
