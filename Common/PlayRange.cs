using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public struct PlayRange:IEquatable<PlayRange>
    {
        public static readonly PlayRange Empty = new PlayRange();

        internal static bool IsUnbroken(PlayRange range1, PlayRange range2)
        {
            return (range1.StopPosition == range2.StartPosition || range1.StartPosition == range2.StopPosition);
        }

        internal static void Break(PlayRange range, TimeSpan duration, out PlayRange first, out PlayRange second)
        {
            if (duration >= range.Duration)
            {
                throw new ArgumentOutOfRangeException("duration", 
                    string.Format("<{0}>无效，必须小于{1}。", duration,range.Duration));
            }

            first = new PlayRange(range.StartPosition, duration);
            var breakPos = first.StopPosition;
            second = new PlayRange(breakPos, range.StopPosition.Subtract(breakPos));
        }

        internal static PlayRange Merge(PlayRange range1, PlayRange range2)
        {
            var duration = range1.Duration + range2.Duration;

            if(range1.StopPosition == range2.StartPosition)
            {
                return new PlayRange(range1.StartPosition, duration);
            }
            else if(range1.StartPosition == range2.StopPosition)
            {
                return new PlayRange(range2.StartPosition, duration);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private TimeSpan _startPos;
        private TimeSpan _duration;

        public PlayRange(TimeSpan startPos,TimeSpan duration)
        {
            if (startPos < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("startPos", 
                    string.Format("<{0}>无效，不能小于TimeSpan.Zero。",startPos));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("duration",
                    string.Format("<{0}>无效，不能小于TimeSpan.Zero。", duration));
            }

            _startPos = startPos;
            _duration = duration;
        }

        public PlayRange(TimeSpan duration) : this(TimeSpan.Zero,duration)
        {
        }

        public TimeSpan StartPosition { get { return _startPos; } }

        public TimeSpan StopPosition { get { return _startPos.Add(_duration); } }

        public TimeSpan Duration { get { return _duration; } }



        #region 重写基类函数
        public override string ToString()
        {
            return string.Format("({0}, {1})", this.StartPosition, this.StopPosition);
        }

        public override bool Equals(object obj)
        {
            return Equals((PlayRange)obj);
        }

        public override int GetHashCode()
        {
            return _startPos.GetHashCode() ^ _duration.GetHashCode();
        }
        #endregion

        #region IEquatable<PlayRange>接口实现
        public bool Equals(PlayRange other)
        {
            return _startPos == other._startPos && _duration == other._duration;
        }
        #endregion
        
        #region 等于和不等于操作符重载
        public static bool operator ==(PlayRange left, PlayRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayRange left, PlayRange right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
