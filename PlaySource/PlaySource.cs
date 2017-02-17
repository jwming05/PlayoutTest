using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public class PlaySource : IPlaySource
    {
        public static IPlaySource Create(MarkableMediaSource markableMediaSource)
        {
            return new PlaySource(markableMediaSource);
        }

        internal static bool CanMerge(IPlaySource playSource1, IPlaySource playSource2)
        {
            // TODO: 外部通道和自动垫片是否允许合并？
            var segment1 = playSource1 as PlaySourceSegment;
            if (segment1 == null) return false;

            var segment2 = playSource2 as PlaySourceSegment;
            if (segment1 == null) return false;

            if (segment1.PlaySource != segment2.PlaySource)
            {
                return false;
            }

            return segment1.PlaySource.MediaSource.CanMerge(segment1.PlayRange, segment2.PlayRange);
        }

        internal static IPlaySource Merge(IPlaySource playSource1, IPlaySource playSource2)
        {
            if (!CanMerge(playSource1, playSource2))
            {
                throw new InvalidOperationException();
            }

            var segment1 = (PlaySourceSegment)playSource1;
            var segment2 = (PlaySourceSegment)playSource2;

            PlayRange range = segment1.PlaySource.MediaSource.Merge(segment1.PlayRange, segment2.PlayRange);

            range = segment1.PlaySource.AdjustPlayRange(range);

            return Create(segment1.PlaySource, range);
        }

        internal static bool IsSegment(IPlaySource playSource)
        {
            return playSource is PlaySourceSegment;
        }

        internal static void Split(IPlaySource playSource, TimeSpan duration, out IPlaySource first, out IPlaySource second)
        {
            var segment = playSource as PlaySourceSegment;
            PlayRange range1, range2;
            PlayRange.Break(playSource.PlayRange, duration, out range1, out range2);

            PlaySource source;
            if (segment != null)
            {
                source = segment.PlaySource;                
            }
            else
            {
                source = (PlaySource)playSource;
            }

            first = new PlaySourceSegment(source, range1);
            second = new PlaySourceSegment(source, range2);
        }

        private static IPlaySource Create(PlaySource playSource, PlayRange range)
        {
            if (range.Duration < playSource.PlayRange.Duration)
            {
                return new PlaySourceSegment(playSource, range);
            }
            else
            {
                return playSource;
            }
        }

        private MarkableMediaSource _markableMediaSource;

        private PlaySource(MarkableMediaSource markableMediaSource)
        {
            MarkableMediaSource.Validate(markableMediaSource);

            _markableMediaSource = markableMediaSource;
        }

        public PlayRange PlayRange
        {
            get;set;
        }

        public IMediaSource MediaSource { get; set; }

        public string Title
        {
            get
            {
                return this.MediaSource.Title;
            }
        }

        class PlaySourceSegment : IPlaySource
        {
            public PlaySourceSegment(PlaySource playSource, PlayRange playRange)
            {
                if (playSource == null)
                {
                    throw new ArgumentNullException("playSource");
                }

                playRange = playSource.AdjustPlayRange(playRange);

                if (playRange.Duration >= playSource.PlayRange.Duration)
                {
                    throw new ArgumentException(
                        string.Format("片断的时长必须小于源时长。", playRange.Duration, playSource.PlayRange.Duration));
                }

                this.PlayRange = playRange;
                this.PlaySource = playSource;

                //if (playSource.IsSegment())
                //{
                //    this.PlaySource = ((PlaySourceSegment)playSource).PlaySource;
                //}
                //else
                //{

                //    this.PlaySource = (PlaySource)playSource;
                //    //this.PlayRange = this.PlaySource.AdjustPlayRange(playRange);
                //}
            }

            public IMediaSource MediaSource
            {
                get { return this.PlaySource.MediaSource; }
            }

            public PlayRange PlayRange
            {
                get; set;
            }

            public PlaySource PlaySource { get; private set; }

            public string Title
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        
    }
}
