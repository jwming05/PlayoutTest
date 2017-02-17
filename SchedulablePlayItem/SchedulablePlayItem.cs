using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public abstract class SchedulablePlayItem: ISchedulablePlayItem
    {
        public static SchedulablePlayItem Create(DateTime? startTime, ScheduleMode scheduleMode, MarkableMediaSource mediaSource)
        {
            IPlaySource source = PlayoutTest.PlaySource.Create(mediaSource);

            return Create(startTime, scheduleMode, source);
        }

        public static SchedulablePlayItem Create(DateTime? startTime, ScheduleMode scheduleMode, IPlaySource playSource)
        {
            switch (scheduleMode)
            {
                case ScheduleMode.Timing:
                case ScheduleMode.TimingBreak:
                    if (startTime == null)
                    {
                        throw new ArgumentNullException("startTime");
                    }
                    return new TimingSchedulablePlayItem(startTime.Value, playSource, scheduleMode);
                case ScheduleMode.Auto:
                    break;
                default:
                    break;
            }
            //playSource
            throw new System.ComponentModel.InvalidEnumArgumentException("scheduleMode", (int)scheduleMode, typeof(ScheduleMode));
        }

        public abstract ScheduleInfo ScheduleInfo
        {
            get; set;
        }

        public IPlaySource PlaySource
        {
            get;private set;
        }

        public virtual string Title
        {
            get { return this.PlaySource.Title; }
        }

        //public Guid Id { get; set; }

        //public ScheduleInfo ScheduleInfo { get; set; }

        //public IPlaySource PlaySource { get; set; }

        class TimingSchedulablePlayItem : SchedulablePlayItem
        {
            private ScheduleInfo _scheduleInfo;

            public TimingSchedulablePlayItem(DateTime startTime, IPlaySource playSource, ScheduleMode scheduleMode)
            {
            }

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
        }

        class AutoSchedulablePlayItem : SchedulablePlayItem
        {
            private AutoSchedulablePlayItem(MarkableMediaSource mediaSource)
            {
            }

            public override ScheduleInfo ScheduleInfo
            {
                get;set;
            }
        }
    }
}
