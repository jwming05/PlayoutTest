using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public class PlayScheduler
    {
        private ScheduleConfiguration _config;

        private IDateTimeService _dateTimeService;
        private IScheduleQueue _playItemQueue;

        private IPlayerCreator _playerCreator;

        //private IPlayer _currentPlayer;
        private IPlayer _nextPlayer;

        private void Schedule()
        {
            if (_nextPlayer != null)
            {
                if (CanStart(_nextPlayer))
                {
                    _nextPlayer.Start();
                    _nextPlayer = null;
                }
            }
            else
            {
                IPlayItem nextPlayItem = FindNextPlayItem();
                if (nextPlayItem != null)
                {
                    _nextPlayer = _playerCreator.Create(nextPlayItem);
                }
            }
        }

        private IPlayItem FindNextPlayItem()
        {
            var nextItem = _playItemQueue.Peek();

            if (nextItem != null && CanPrepare(nextItem))
            {
                _playItemQueue.Take(nextItem);
                return nextItem;
            }
            return null;            
        }

        private bool CanPrepare(IPlayItem nextItem)
        {
            DateTime startTime = nextItem.ScheduleInfo.StartTime;

            var now = _dateTimeService.LocalNow();

            return startTime.Subtract(now) <= _config.MaxPreparationTime;
        }

        private bool CanStart(IPlayer player)
        {
            DateTime startTime = player.PlayItem.ScheduleInfo.StartTime;

            var now = _dateTimeService.LocalNow();

            TimeSpan diff;
            if (now >= startTime)
            {
                // 已经过了开始时间，这时超过的时间不应该太多，太多则表示出现错误。
                diff = now.Subtract(startTime);
            }
            else
            {
                // 还未到开始时间。
                diff = startTime.Subtract(now);
            }


            return diff <= _config.PlayTimeTolerance;
        }

        public interface IScheduleQueue
        {
            IPlayItem Peek();
            void Take(IPlayItem item); 
        }
    }

    public class ScheduleConfiguration
    {
        /// <summary>
        /// 获取或设置播放项的开播时间容差。
        /// </summary>
        public TimeSpan PlayTimeTolerance { get; set; }

        /// <summary>
        /// 获取或设置播放项的最大提前准备时间。
        /// </summary>
        public TimeSpan MaxPreparationTime { get; set; } 
    }
}
