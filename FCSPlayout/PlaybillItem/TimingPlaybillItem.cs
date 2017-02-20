using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    /// <summary>
    /// 表示一个定时播或定时插播。
    /// </summary>
    internal class TimingPlaybillItem : PlaybillItem //, IPlaylistItem
    {
        private readonly ScheduleInfo _scheduleInfo;

        internal static TimingPlaybillItem Create(DateTime startTime, IPlaySource playSource, bool isBreak)
        {
            return new TimingPlaybillItem(startTime, new NormalPlaySource(playSource), isBreak);
        }

        private TimingPlaybillItem(DateTime startTime,NormalPlaySource playSource,bool isBreak)
            :base(playSource, isBreak ? ScheduleMode.TimingBreak : ScheduleMode.Timing)
        {
            _scheduleInfo = new ScheduleInfo(startTime, this.PlaySource.PlayRange.Duration,this.ScheduleMode);
        }

        //public PlaybillItem PlaybillItem
        //{
        //    get
        //    {
        //        return this;
        //    }
        //}

        public override ScheduleInfo ScheduleInfo
        {
            get
            {
                return _scheduleInfo;
            }

            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsSegment
        {
            get
            {
                return false;
            }
        }

        public override bool IsFirstSegment
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsLastSegment
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public override bool CanMerge(PlaybillItem nextItem)
        {
            return false;
        }

        public override PlaybillItem Merge(PlaybillItem nextItem)
        {
            throw new InvalidOperationException();
        }

        public override void Split(TimeSpan duration, out PlaybillItem first, out PlaybillItem second)
        {
            throw new InvalidOperationException();
        }
    }
}
