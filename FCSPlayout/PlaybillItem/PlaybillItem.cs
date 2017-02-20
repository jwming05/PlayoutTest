using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public abstract class PlaybillItem : IPlaylistItem // : IPlaybillItem
    {
        internal static PlaybillItem CreateAutoPadding(TimeSpan duration)
        {
            PlaySource source = FCSPlayout.NormalPlaySource.CreateAutoPadding(duration);
            return CreateAuto(source);
        }

        internal static PlaybillItem CreateAuto(IPlaySource playSource)
        {
            return AutoPlaybillItem.Create(playSource);
        }

        internal static PlaybillItem CreateTiming(DateTime startTime, IPlaySource playSource, bool isBreak)
        {
            return TimingPlaybillItem.Create(startTime, playSource, isBreak);
        }

        // 设为internal来阻止外部派生。
        internal PlaybillItem(PlaySource playSource, ScheduleMode scheduleMode)
        {
            if (playSource == null)
            {
                throw new ArgumentNullException("playSource");
            }

            if (!Enum.IsDefined(typeof(ScheduleMode), scheduleMode))
            {
                throw new System.ComponentModel.InvalidEnumArgumentException("scheduleMode", (int)scheduleMode,typeof(ScheduleMode));
            }
            this.PlaySource = playSource;
            this.ScheduleMode = scheduleMode;
        }

        public PlaySource PlaySource
        {
            get;private set;
        }
        public abstract ScheduleInfo ScheduleInfo { get; set; }
        public ScheduleMode ScheduleMode
        {
            get;private set;
        }

        public virtual string Title
        {
            get
            {
                return this.PlaySource.Title;
            }
        }

        PlaybillItem IPlaylistItem.PlaybillItem
        {
            get
            {
                return this;
            }
        }

        public abstract bool IsSegment { get; }
        public abstract bool IsFirstSegment { get; }
        public abstract bool IsLastSegment { get; }

        public abstract bool CanMerge(PlaybillItem nextItem);
        public abstract PlaybillItem Merge(PlaybillItem nextItem);
        public abstract void Split(TimeSpan duration, out PlaybillItem first, out PlaybillItem second);

        
    }
}
