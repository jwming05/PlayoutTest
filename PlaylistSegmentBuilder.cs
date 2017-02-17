using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    class PlaylistSegmentBuilder : IDisposable
    {
        private ISchedulablePlayItem _beginItem;
        private ISchedulablePlayItem _endItem;

        private List<ISchedulablePlayItem> _autoList = new List<ISchedulablePlayItem>();
        private List<ISchedulablePlayItem> _breakList = new List<ISchedulablePlayItem>();

        public void Dispose()
        {
        }

        public void SetTimingRange(ISchedulablePlayItem beginItem, ISchedulablePlayItem endItem)
        {
            if(beginItem==null && endItem == null)
            {
                throw new ArgumentNullException("", "beginItem和endItem不能全为null。");
            }

            if(beginItem!=null && beginItem.ScheduleInfo.ScheduleMode != ScheduleMode.Timing)
            {
                throw new ArgumentException("必须是定时播。", "beginItem");
            }

            if (endItem != null && endItem.ScheduleInfo.ScheduleMode != ScheduleMode.Timing)
            {
                throw new ArgumentException("必须是定时播。", "endItem");
            }

            if(beginItem!=null && endItem != null)
            {
                if (beginItem.ScheduleInfo.StopTime > endItem.ScheduleInfo.StartTime)
                {
                    throw new ArgumentException("beginItem的结束时间不能大于endItem的开始时间。");
                }
            }
            
            _beginItem = beginItem;
            _endItem = endItem;
        }

        public void Add(ISchedulablePlayItem schedulablePlayItem)
        {
            var scheduleMode = schedulablePlayItem.ScheduleInfo.ScheduleMode;
            if (scheduleMode != ScheduleMode.Auto && scheduleMode != ScheduleMode.TimingBreak)
            {
                    throw new ArgumentException("必须是顺播或定时插播。", "schedulablePlayItem");
            }

            if (scheduleMode == ScheduleMode.Auto)
            {
                AddAuto(schedulablePlayItem);
            }
            else
            {
                AddTimingBreak(schedulablePlayItem);
            }
        }

        private void Build()
        {
            DateTime startTime, stopTime=DateTime.MaxValue;

            if (_beginItem!=null && _endItem != null)
            {
                startTime = _beginItem.ScheduleInfo.StopTime;
                stopTime = _endItem.ScheduleInfo.StartTime;
            }
            else
            {
                if (_beginItem != null)
                {
                    startTime = _beginItem.ScheduleInfo.StopTime;
                }
                else // _endItem != null
                {
                    stopTime = _endItem.ScheduleInfo.StartTime;

                    startTime = stopTime;
                    if(_autoList.Count>0 && _breakList.Count > 0)
                    {
                        var t1 = _autoList[0].ScheduleInfo.StartTime;
                        var t2 = _breakList[0].ScheduleInfo.StartTime;
                        startTime = t1 < t2 ? t1 : t2;
                    }
                    else if (_autoList.Count > 0)
                    {
                        startTime = _autoList[0].ScheduleInfo.StartTime;
                    }
                    else if (_breakList.Count > 0)
                    {
                        startTime = _breakList[0].ScheduleInfo.StartTime;
                    }
                    else
                    {
                        // nothing to do
                        // startTime == stopTime
                    }

                    startTime = startTime > stopTime ? stopTime : startTime;
                }
            }

            Build(startTime, stopTime);
        }

        private void Build(DateTime startTime, DateTime stopTime)
        {
            ISchedulablePlayItem autoItem = null;
            ISchedulablePlayItem breakItem = null;

            while (startTime < stopTime)
            {
                

                if (autoItem == null && _autoList.Count > 0)
                {
                    autoItem = _autoList[0];
                    _autoList.RemoveAt(0);
                }

                if (breakItem==null && _breakList.Count > 0)
                {
                    autoItem = _breakList[0];
                    _breakList.RemoveAt(0);
                }

                if(autoItem!=null && breakItem != null)
                {
                    if (startTime < breakItem.ScheduleInfo.StartTime)
                    {
                        var duration = breakItem.ScheduleInfo.StartTime.Subtract(startTime);
                        if (duration >= autoItem.PlaySource.PlayRange.Duration)
                        {
                            var info = autoItem.ScheduleInfo;
                            info = new ScheduleInfo(startTime, autoItem.PlaySource.PlayRange.Duration, info.ScheduleMode);
                            autoItem.ScheduleInfo = info;

                            // insert auto item

                            autoItem = null;
                            startTime = info.StopTime;
                        }
                        else
                        {
                            ISchedulablePlayItem first = null, second = null;
                            Split(autoItem, duration, out first, out second);

                            var info = autoItem.ScheduleInfo;
                            info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
                            first.ScheduleInfo = info;

                            // insert first item

                            autoItem = second;
                            startTime = info.StopTime;
                        }
                    }
                    else
                    {
                        // insert breakItem item

                        breakItem = null;
                        startTime = breakItem.ScheduleInfo.StopTime;
                    }
                }
                else if (autoItem != null) // breakItem == null
                {
                    var duration = stopTime.Subtract(startTime);
                    if (duration >= autoItem.PlaySource.PlayRange.Duration)
                    {
                        var info = autoItem.ScheduleInfo;
                        info = new ScheduleInfo(startTime, autoItem.PlaySource.PlayRange.Duration, info.ScheduleMode);
                        autoItem.ScheduleInfo = info;

                        // insert auto item

                        autoItem = null;
                        startTime = info.StopTime;
                    }
                    else
                    {
                        autoItem.Truncate(stopTime);

                        // insert autoItem item

                        startTime = autoItem.ScheduleInfo.StopTime;
                        autoItem = null;
                        
                    }
                }
                else if (breakItem != null) // autoItem == null
                {
                    if (startTime < breakItem.ScheduleInfo.StartTime)
                    {
                        var duration = breakItem.ScheduleInfo.StartTime.Subtract(startTime);

                        // create auto padding item

                        ISchedulablePlayItem temp = CreateAutoPaddingItem(startTime, duration);

                        // insert temp

                        startTime = breakItem.ScheduleInfo.StartTime;
                    }
                    else
                    {
                        // insert breakItem item

                        breakItem = null;
                        startTime = breakItem.ScheduleInfo.StopTime;
                    }
                }
                else
                {
                    if (stopTime != DateTime.MaxValue)
                    {
                        ISchedulablePlayItem temp = CreateAutoPaddingItem(startTime, stopTime.Subtract(startTime));

                        // insert temp
                    }


                    startTime = stopTime;
                }
            }
        }

        private ISchedulablePlayItem CreateAutoPaddingItem(DateTime startTime, TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        private void Split(ISchedulablePlayItem autoItem, TimeSpan duration, 
            out ISchedulablePlayItem first, out ISchedulablePlayItem second)
        {
            IPlaySource s1, s2;
            PlaySource.Split(autoItem.PlaySource, duration, out s1, out s2);

            first = SchedulablePlayItem.Create(null, autoItem.ScheduleInfo.ScheduleMode, s1);
            second= SchedulablePlayItem.Create(null, autoItem.ScheduleInfo.ScheduleMode, s2);
        }

        private void AddTimingBreak(ISchedulablePlayItem schedulablePlayItem)
        {
            _breakList.Add(schedulablePlayItem);
        }

        private void AddAuto(ISchedulablePlayItem schedulablePlayItem)
        {
            // TODO: 是否删除自动垫片。
            ISchedulablePlayItem prevItem = _autoList.Count > 0 ? _autoList[_autoList.Count - 1] : null;
            if (prevItem!=null && CanMerge(prevItem, schedulablePlayItem))
            {
                _autoList[_autoList.Count - 1] = Merge(prevItem, schedulablePlayItem);
            }
            else
            {
                _autoList.Add(schedulablePlayItem);
            }
        }

        private ISchedulablePlayItem Merge(ISchedulablePlayItem item1, ISchedulablePlayItem item2)
        {
            IPlaySource source = PlaySource.Merge(item1.PlaySource, item2.PlaySource);

            return SchedulablePlayItem.Create(item1.ScheduleInfo.StartTime, item1.ScheduleInfo.ScheduleMode, source);
            //throw new NotImplementedException();
        }

        private bool CanMerge(ISchedulablePlayItem item1, ISchedulablePlayItem item2)
        {
            return PlaySource.CanMerge(item1.PlaySource, item2.PlaySource);
        }
    }
}
