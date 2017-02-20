using System;

namespace FCSPlayout
{
    public struct ScheduleInfo:IEquatable<ScheduleInfo>
    {
        public static readonly ScheduleInfo Empty = new ScheduleInfo();

        public static bool IsEmpty(ScheduleInfo info)
        {
            return info == Empty;
        }

        public ScheduleInfo(DateTime startTime, TimeSpan duration, ScheduleMode scheduleMode)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("duration", 
                    string.Format("<{0}>无效，不能小于TimeSpan.Zero。", duration));
            }

            if (!Enum.IsDefined(typeof(ScheduleMode), scheduleMode))
            {
                throw new System.ComponentModel.InvalidEnumArgumentException("scheduleMode", (int)scheduleMode, typeof(ScheduleMode));
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

        #region IEquatable<ScheduleInfo>接口实现
        public bool Equals(ScheduleInfo other)
        {
            return this.StartTime == other.StartTime && this.ScheduleMode == other.ScheduleMode && this.Duration == other.Duration;
        }
        #endregion

        #region 重写基类函数
        public override bool Equals(object obj)
        {
            return Equals((ScheduleInfo)obj);
        }

        public override int GetHashCode()
        {
            return this.StartTime.GetHashCode() ^ this.ScheduleMode.GetHashCode() ^ this.Duration.GetHashCode();
        }
        #endregion

        #region 等于和不等于操作符重载
        public static bool operator ==(ScheduleInfo left, ScheduleInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScheduleInfo left, ScheduleInfo right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
