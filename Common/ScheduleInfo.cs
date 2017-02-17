using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public struct ScheduleInfo
    {
        public ScheduleInfo(DateTime startTime, TimeSpan duration, ScheduleMode scheduleMode)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("duration", 
                    string.Format("<{0}>无效，不能小于TimeSpan.Zero。", duration));
            }

            StartTime = startTime;
            Duration = duration;
            ScheduleMode = scheduleMode;
        }

        public DateTime StartTime { get; private set; }

        public ScheduleMode ScheduleMode { get; private set; }

        public TimeSpan Duration { get; private set; }

        public DateTime StopTime
        {
            get { return this.StartTime.Add(this.Duration); }
        }
    }
}
