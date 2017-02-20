using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    class PlaylistSegmentBuilder : IDisposable
    {
        //private IPlaylistItem _beginItem;
        //private IPlaylistItem _endItem;

        //private DateTime _startTime;
        //private DateTime _stopTime;

        private List<PlaybillItem> _autoList = new List<PlaybillItem>();
        private List<PlaybillItem> _breakList = new List<PlaybillItem>();
        private List<PlaybillItem> _resultList = new List<PlaybillItem>();

        public void Dispose()
        {
        }

        //public void Init()
        //{

        //}
        //public void SetTimingRange(IPlaylistItem beginItem, IPlaylistItem endItem)
        //{
        //    if(beginItem==null && endItem == null)
        //    {
        //        throw new ArgumentNullException("", "beginItem和endItem不能全为null。");
        //    }

        //    if(beginItem!=null && beginItem.ScheduleInfo.ScheduleMode != ScheduleMode.Timing)
        //    {
        //        throw new ArgumentException("必须是定时播。", "beginItem");
        //    }

        //    if (endItem != null && endItem.ScheduleInfo.ScheduleMode != ScheduleMode.Timing)
        //    {
        //        throw new ArgumentException("必须是定时播。", "endItem");
        //    }

        //    if(beginItem!=null && endItem != null && beginItem.ScheduleInfo.StopTime > endItem.ScheduleInfo.StartTime)
        //    {
        //        throw new ArgumentException("beginItem的结束时间不能大于endItem的开始时间。");
        //    }
            
        //    _beginItem = beginItem;
        //    _endItem = endItem;
        //}

        public void Add(PlaybillItem playbillItem)
        {
            if (playbillItem == null)
            {
                throw new ArgumentNullException("playbillItem");
            }

            var scheduleMode = playbillItem.ScheduleMode;
            if (scheduleMode != ScheduleMode.Auto && scheduleMode != ScheduleMode.TimingBreak)
            {
                    throw new ArgumentException("必须是顺播或定时插播。", "playbillItem");
            }

            if (scheduleMode == ScheduleMode.Auto)
            {
                AddAuto(playbillItem);
            }
            else
            {
                _breakList.Add(playbillItem);
            }
        }

        private void AddAuto(PlaybillItem playbillItem)
        {
            if (!playbillItem.IsAutoPadding()) // 移除自动垫片。
            {                
                PlaybillItem prevItem = _autoList.Count > 0 ? _autoList[_autoList.Count - 1] : null;
                if (prevItem != null && prevItem.CanMerge(playbillItem)) //PlaySource.CanMerge(prevItem.PlaySource, playbillItem.PlaySource)
                {
                    _autoList[_autoList.Count - 1] = prevItem.Merge(playbillItem); // Merge(prevItem, playbillItem);
                }
                else
                {
                    _autoList.Add(playbillItem);
                }
            }
            
        }

        //public IList<IPlaylistItem> Build()
        //{
        //    DateTime startTime, stopTime;

        //    if (_beginItem!=null && _endItem != null)
        //    {
        //        startTime = _beginItem.ScheduleInfo.StopTime;
        //        stopTime = _endItem.ScheduleInfo.StartTime;
        //    }
        //    else
        //    {
        //        if (_beginItem != null)
        //        {
        //            startTime = _beginItem.ScheduleInfo.StopTime;
        //            stopTime = DateTime.MaxValue;
        //        }
        //        else // _endItem != null
        //        {
        //            stopTime = _endItem.ScheduleInfo.StartTime;


        //            if(_autoList.Count>0 && _breakList.Count > 0)
        //            {
        //                var autoStartTime = _autoList[0].ScheduleInfo.StartTime;
        //                var breakStartTime = _breakList[0].ScheduleInfo.StartTime;
        //                startTime = autoStartTime < breakStartTime ? autoStartTime : breakStartTime;
        //            }
        //            else if (_autoList.Count > 0)
        //            {
        //                startTime = _autoList[0].ScheduleInfo.StartTime;
        //            }
        //            else if (_breakList.Count > 0)
        //            {
        //                startTime = _breakList[0].ScheduleInfo.StartTime;
        //            }
        //            else
        //            {
        //                startTime = stopTime;
        //            }

        //            startTime = startTime > stopTime ? stopTime : startTime;
        //        }
        //    }

        //    var result = new List<IPlaylistItem>();
        //    Build(startTime, stopTime, result);
        //    return result;
        //}

        public IList<PlaybillItem> Build(DateTime startTime, DateTime stopTime)
        {
            Build(startTime, stopTime, _resultList);
            return _resultList;
        }

        private void Build(DateTime startTime, DateTime stopTime, IList<PlaybillItem> result)
        {
            PlaybillItem autoItem = null;
            PlaybillItem breakItem = null;

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
                            duration = autoItem.PlaySource.PlayRange.Duration;
                            var info = autoItem.ScheduleInfo;
                            info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
                            autoItem.ScheduleInfo = info;

                            result.Add(autoItem);
                            autoItem = null;
                            startTime = info.StopTime;
                        }
                        else
                        {
                            PlaybillItem first = null, second = null;
                            // 注：autoItem可能本身是个片断。
                            Split(autoItem, duration, out first, out second);

                            var info = autoItem.ScheduleInfo;
                            info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
                            first.ScheduleInfo = info;

                            result.Add(first);
                            autoItem = second;
                            startTime = info.StopTime;
                        }
                    }
                    else
                    {
                        Debug.Assert(startTime == breakItem.ScheduleInfo.StartTime);

                        result.Add(breakItem);
                        breakItem = null;
                        startTime = breakItem.ScheduleInfo.StopTime;
                    }
                }
                else if (autoItem != null) // breakItem == null
                {
                    var duration = stopTime.Subtract(startTime);
                    if (duration >= autoItem.PlaySource.PlayRange.Duration)
                    {
                        duration = autoItem.PlaySource.PlayRange.Duration;
                        var info = autoItem.ScheduleInfo;
                        info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
                        autoItem.ScheduleInfo = info;

                        result.Add(autoItem);
                        autoItem = null;
                        startTime = info.StopTime;
                    }
                    else
                    {
                        var info = autoItem.ScheduleInfo;
                        info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
                        autoItem.ScheduleInfo = info;

                        result.Add(autoItem);
                        autoItem = null;
                        startTime = info.StopTime;
                    }
                }
                else if (breakItem != null) // autoItem == null
                {
                    if (startTime < breakItem.ScheduleInfo.StartTime)
                    {
                        var duration = breakItem.ScheduleInfo.StartTime.Subtract(startTime);
                        PlaybillItem autoPaddingItem = CreateAutoPaddingItem(startTime, duration);
                        result.Add(autoPaddingItem);

                        startTime = autoPaddingItem.ScheduleInfo.StopTime; // breakItem.ScheduleInfo.StartTime;
                    }
                    else
                    {
                        Debug.Assert(startTime == breakItem.ScheduleInfo.StartTime);

                        result.Add(breakItem);
                        startTime = breakItem.ScheduleInfo.StopTime;
                        breakItem = null;
                    }
                }
                else
                {
                    if (stopTime != DateTime.MaxValue)
                    {
                        PlaybillItem autoPaddingItem = CreateAutoPaddingItem(startTime, stopTime.Subtract(startTime));
                        result.Add(autoPaddingItem);
                    }


                    startTime = stopTime;
                }
            }

            Debug.Assert(_breakList.Count == 0);
            while (_autoList.Count > 0)
            {
                autoItem = _autoList[0];
                _autoList.RemoveAt(0);

                var info = autoItem.ScheduleInfo;
                info = new ScheduleInfo(startTime, TimeSpan.Zero, info.ScheduleMode);
                autoItem.ScheduleInfo = info;

                result.Add(autoItem);
            }
        }

        private PlaybillItem CreateAutoPaddingItem(DateTime startTime, TimeSpan duration)
        {
            var result= PlaybillItem.CreateAutoPadding(duration);
            result.ScheduleInfo = new ScheduleInfo(startTime, duration, ScheduleMode.Auto);
            return result;
        }

        private void Split(PlaybillItem autoItem, TimeSpan duration, 
            out PlaybillItem first, out PlaybillItem second)
        {
            autoItem.Split(duration, out first, out second);
        }
    }
}
