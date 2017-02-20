using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    internal sealed partial class AutoPlaybillItem:PlaybillItem
    {
        private ScheduleInfo _scheduleInfo;

        internal static AutoPlaybillItem Create(IPlaySource playSource)
        {
            return CreateInternal(new NormalPlaySource(playSource));
        }

        private static AutoPlaybillItem CreateInternal(PlaySource playSource)
        {
            var segment = playSource as PlaySourceSegment;
            if (segment != null)
            {
                return new AutoPlaybillItem(segment);
            }
            else
            {
                return new AutoPlaybillItem((NormalPlaySource)playSource);
            }
        }

        private AutoPlaybillItem(NormalPlaySource playSource)
            :base(playSource,ScheduleMode.Auto)
        {
        }

        private AutoPlaybillItem(PlaySourceSegment playSource)
            : base(playSource, ScheduleMode.Auto)
        {
        }

        public override ScheduleInfo ScheduleInfo
        {
            get { return _scheduleInfo; }
            set
            {
                if (ScheduleInfo.IsEmpty(value))
                {
                    throw new ArgumentException("value值未初始化。");
                }

                if (value.ScheduleMode != ScheduleMode.Auto)
                {
                    throw new ArgumentException("value.ScheduleMode必须是ScheduleMode.Auto。");
                }

                _scheduleInfo = value;
            }
        }

        public override bool IsSegment
        {
            get
            {
                return this.PlaySource is PlaySourceSegment;
            }
        }

        public override bool IsFirstSegment
        {
            get
            {
                return this.IsSegment && ((PlaySourceSegment)this.PlaySource).IsFirstSegment;
            }
        }

        public override bool IsLastSegment
        {
            get
            {
                return this.IsSegment && ((PlaySourceSegment)this.PlaySource).IsLastSegment;
            }
        }

        public override bool CanMerge(PlaybillItem nextItem)
        {
            if (nextItem == null)
            {
                throw new ArgumentNullException("nextItem");
            }

            // Note: 是否在自动垫片上操作？

            var segment = this.PlaySource as PlaySourceSegment;
            return segment != null ? segment.CanMerge(nextItem.PlaySource) : false;
        }

        public override PlaybillItem Merge(PlaybillItem nextItem)
        {
            if (!CanMerge(nextItem))
            {
                throw new InvalidOperationException();
            }

            // Note: 是否在自动垫片上操作？

            PlaySource mergedSource = ((PlaySourceSegment)this.PlaySource).Merge(nextItem.PlaySource);

            return CreateInternal(mergedSource);
        }

        public override void Split(TimeSpan duration, out PlaybillItem first, out PlaybillItem second)
        {
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("duration", string.Format("<{0}>无效，必须大于TimeSpan.Zero。",duration));
            }

            // Note: 是否在自动垫片上操作？

            PlayRange firstRange, secondRange;
            this.PlaySource.Break(duration, out firstRange, out secondRange);

            var segment = this.PlaySource as PlaySourceSegment;
            NormalPlaySource source= segment != null ? segment.PlaySource : (NormalPlaySource)this.PlaySource;

            var firstSource = PlaySourceSegment.Create(source, firstRange);
            first = CreateInternal(firstSource);

            var secondSource = PlaySourceSegment.Create(source, secondRange);
            second = CreateInternal(secondSource);
        }
    }

    internal sealed partial class AutoPlaybillItem
    {
        class PlaySourceSegment : PlaySource // IPlaySource
        {
            public static PlaySource Create(NormalPlaySource playSource, PlayRange playRange)
            {
                // Note: 是否在自动垫片上操作？

                if (playSource == null)
                {
                    throw new ArgumentNullException("playSource");
                }

                playRange = playSource.Adjust(playRange);

                if (playRange == playSource.PlayRange)
                {
                    return playSource;
                }

                return new PlaySourceSegment(playSource, playRange);
            }

            private PlaySourceSegment(NormalPlaySource playSource, PlayRange playRange)
            {
                this.PlayRange = playRange;
                this.PlaySource = playSource;
            }

            internal NormalPlaySource PlaySource { get; private set; }
            internal bool IsFirstSegment
            {
                get { return this.PlayRange.StartPosition == this.PlaySource.PlayRange.StartPosition; }
            }

            internal bool IsLastSegment
            {
                get { return this.PlayRange.StopPosition == this.PlaySource.PlayRange.StopPosition; }
            }

            internal bool CanMerge(PlaySource nextSource)
            {
                var nextSegment = nextSource as PlaySourceSegment;
                if (nextSegment == null) return false;

                Debug.Assert(nextSegment != this);

                if (nextSegment.PlaySource != nextSegment.PlaySource)
                {
                    return false;
                }

                return this.MediaSource.CanMerge(this.PlayRange, nextSegment.PlayRange);
            }

            internal PlaySource Merge(PlaySource nextSource)
            {
                if (!CanMerge(nextSource))
                {
                    throw new InvalidOperationException();
                }

                PlayRange range = this.MediaSource.Merge(this.PlayRange, ((PlaySourceSegment)nextSource).PlayRange);

                range = this.PlaySource.Adjust(range); // 确保范围不超出this.PlaySource.PlayRange

                return Create(this.PlaySource, range);
            }
        }
    }
}
