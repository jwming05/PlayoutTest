using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    class PlaylistSegmentBuildData : IDisposable
    {
        private List<PlaybillItem> _autoList = new List<PlaybillItem>();
        private List<PlaybillItem> _breakList = new List<PlaybillItem>();
        private List<PlaybillItem> _resultList = new List<PlaybillItem>();
        private DateTime? _beginTime;
        private DateTime? _endTime;
        private PlaybillItem _timingItem;

        public DateTime BeginTime
        {
            get
            {
                return _beginTime.Value;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return _endTime.Value;
            }
        }

        public List<PlaybillItem> Result
        {
            get
            {
                return _resultList;
            }
        }

        internal void Set(DateTime beginTime,DateTime endTime, PlaybillItem timingItem)
        {
            Debug.Assert(beginTime < endTime);

            if (timingItem != null)
            {
                if (timingItem.ScheduleMode != ScheduleMode.Timing)
                {
                    throw new ArgumentException("必须是定时播。", "timingItem");
                }

                Debug.Assert(beginTime <= timingItem.ScheduleInfo.StartTime && endTime>=timingItem.ScheduleInfo.StopTime);
            }

            _beginTime = beginTime;
            _endTime = endTime;

            _timingItem = timingItem;
        }

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
                AddBreak(playbillItem);
                
            }
        }

        public PlaybillItem TakeAuto()
        {
            if (_autoList.Count > 0)
            {
                var result = _autoList[0];
                _autoList.RemoveAt(0);
                return result;
            }

            return null;
        }

        public PlaybillItem TakeTiming()
        {
            PlaybillItem breakItem = null;
            PlaybillItem result = null;
            if (_breakList.Count > 0)
            {
                breakItem = _breakList[0];
            }

            if(_timingItem!=null && breakItem != null)
            {
                if (_timingItem.ScheduleInfo.StartTime < breakItem.ScheduleInfo.StartTime)
                {
                    result = _timingItem;
                    _timingItem = null;                    
                }
                else
                {
                    _breakList.RemoveAt(0);
                    result = breakItem;
                }
            }
            else
            {
                if (_timingItem != null)
                {
                    result = _timingItem;
                    _timingItem = null;                    
                }
                else if (breakItem != null)
                {
                    _breakList.RemoveAt(0);
                    result = breakItem;
                }
            }
            return result;
        }

        private void AddBreak(PlaybillItem breakItem)
        {
            Debug.Assert(_beginTime <= breakItem.ScheduleInfo.StartTime && _endTime >= breakItem.ScheduleInfo.StopTime);
            _breakList.Add(breakItem);
        }

        private void AddAuto(PlaybillItem playbillItem)
        {
            if (!playbillItem.IsAutoPadding()) // 移除自动垫片。
            {
                PlaybillItem prevItem = _autoList.Count > 0 ? _autoList[_autoList.Count - 1] : null;
                if (prevItem != null && prevItem.CanMerge(playbillItem))
                {
                    _autoList[_autoList.Count - 1] = prevItem.Merge(playbillItem);
                }
                else
                {
                    _autoList.Add(playbillItem);
                }
            }

        }

        public void Dispose()
        {
            _autoList.Clear();
            _breakList.Clear();
            _resultList.Clear();
            _timingItem = null;
            _beginTime = null;
            _endTime = null;
        }
    }

    class PlaylistSegmentBuilder
    {
        private PlaylistSegmentBuildData _buildData = new PlaylistSegmentBuildData();

        internal PlaylistSegmentBuildData GetBuidData(DateTime beginTime, DateTime endTime, PlaybillItem timingItem)
        {
            // ensure _buildData has no data.
            _buildData.Set(beginTime, endTime, timingItem);
            return _buildData;
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


        //private void Build(DateTime startTime, DateTime stopTime, IList<PlaybillItem> result)
        //{
        //    PlaybillItem autoItem = null;
        //    PlaybillItem breakItem = null;

        //    while (startTime < stopTime)
        //    {
        //        if (autoItem == null && _autoList.Count > 0)
        //        {
        //            autoItem = _autoList[0];
        //            _autoList.RemoveAt(0);
        //        }

        //        if (breakItem==null && _breakList.Count > 0)
        //        {
        //            autoItem = _breakList[0];
        //            _breakList.RemoveAt(0);
        //        }

        //        if(autoItem!=null && breakItem != null)
        //        {
        //            if (startTime < breakItem.ScheduleInfo.StartTime)
        //            {
        //                var duration = breakItem.ScheduleInfo.StartTime.Subtract(startTime);
        //                if (duration >= autoItem.PlaySource.PlayRange.Duration)
        //                {
        //                    duration = autoItem.PlaySource.PlayRange.Duration;
        //                    var info = autoItem.ScheduleInfo;
        //                    info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
        //                    autoItem.ScheduleInfo = info;

        //                    result.Add(autoItem);
        //                    autoItem = null;
        //                    startTime = info.StopTime;
        //                }
        //                else
        //                {
        //                    PlaybillItem first = null, second = null;
        //                    // 注：autoItem可能本身是个片断。
        //                    Split(autoItem, duration, out first, out second);

        //                    var info = autoItem.ScheduleInfo;
        //                    info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
        //                    first.ScheduleInfo = info;

        //                    result.Add(first);
        //                    autoItem = second;
        //                    startTime = info.StopTime;
        //                }
        //            }
        //            else
        //            {
        //                Debug.Assert(startTime == breakItem.ScheduleInfo.StartTime);

        //                result.Add(breakItem);
        //                breakItem = null;
        //                startTime = breakItem.ScheduleInfo.StopTime;
        //            }
        //        }
        //        else if (autoItem != null) // breakItem == null
        //        {
        //            var duration = stopTime.Subtract(startTime);
        //            if (duration >= autoItem.PlaySource.PlayRange.Duration)
        //            {
        //                duration = autoItem.PlaySource.PlayRange.Duration;
        //                var info = autoItem.ScheduleInfo;
        //                info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
        //                autoItem.ScheduleInfo = info;

        //                result.Add(autoItem);
        //                autoItem = null;
        //                startTime = info.StopTime;
        //            }
        //            else
        //            {
        //                var info = autoItem.ScheduleInfo;
        //                info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
        //                autoItem.ScheduleInfo = info;

        //                result.Add(autoItem);
        //                autoItem = null;
        //                startTime = info.StopTime;
        //            }
        //        }
        //        else if (breakItem != null) // autoItem == null
        //        {
        //            if (startTime < breakItem.ScheduleInfo.StartTime)
        //            {
        //                var duration = breakItem.ScheduleInfo.StartTime.Subtract(startTime);
        //                PlaybillItem autoPaddingItem = CreateAutoPaddingItem(startTime, duration);
        //                result.Add(autoPaddingItem);

        //                startTime = autoPaddingItem.ScheduleInfo.StopTime; // breakItem.ScheduleInfo.StartTime;
        //            }
        //            else
        //            {
        //                Debug.Assert(startTime == breakItem.ScheduleInfo.StartTime);

        //                result.Add(breakItem);
        //                startTime = breakItem.ScheduleInfo.StopTime;
        //                breakItem = null;
        //            }
        //        }
        //        else
        //        {
        //            if (stopTime != DateTime.MaxValue)
        //            {
        //                PlaybillItem autoPaddingItem = CreateAutoPaddingItem(startTime, stopTime.Subtract(startTime));
        //                result.Add(autoPaddingItem);
        //            }


        //            startTime = stopTime;
        //        }
        //    }

        //    Debug.Assert(_breakList.Count == 0);
        //    while (_autoList.Count > 0)
        //    {
        //        autoItem = _autoList[0];
        //        _autoList.RemoveAt(0);

        //        var info = autoItem.ScheduleInfo;
        //        info = new ScheduleInfo(startTime, TimeSpan.Zero, info.ScheduleMode);
        //        autoItem.ScheduleInfo = info;

        //        result.Add(autoItem);
        //    }
        //}

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

        private void Build2(PlaylistSegmentBuildData buildData)
        {
            DateTime startTime = buildData.BeginTime;
            DateTime stopTime = buildData.EndTime;
            IList<PlaybillItem> result = buildData.Result;

            PlaybillItem autoItem = null;
            PlaybillItem timingItem = null;

            while (startTime < stopTime)
            {
                if (autoItem == null)
                {
                    autoItem = buildData.TakeAuto(); // _autoList[0];
                }

                if (timingItem == null)
                {
                    autoItem = buildData.TakeTiming(); // _breakList[0];
                }

                if (autoItem != null && timingItem != null)
                {
                    if (startTime < timingItem.ScheduleInfo.StartTime)
                    {
                        var duration = timingItem.ScheduleInfo.StartTime.Subtract(startTime);
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
                            if (timingItem.ScheduleMode == ScheduleMode.TimingBreak)
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
                            else
                            {
                                // 截短。
                                var info = autoItem.ScheduleInfo;
                                info = new ScheduleInfo(startTime, duration, info.ScheduleMode);
                                autoItem.ScheduleInfo = info;

                                result.Add(autoItem);
                                autoItem = null;
                                startTime = info.StopTime;
                            }
                        }
                    }
                    else
                    {
                        Debug.Assert(startTime == timingItem.ScheduleInfo.StartTime);

                        if (timingItem.ScheduleMode == ScheduleMode.TimingBreak || !autoItem.IsSegment)
                        {
                            result.Add(timingItem);
                            timingItem = null;
                            startTime = timingItem.ScheduleInfo.StopTime;
                        }
                        else
                        {
                            Debug.Assert(!autoItem.IsFirstSegment);
                            // 完全截掉。
                            var info = autoItem.ScheduleInfo;
                            info = new ScheduleInfo(startTime, TimeSpan.Zero, info.ScheduleMode);
                            autoItem.ScheduleInfo = info;

                            result.Add(autoItem);
                            autoItem = null;
                            startTime = info.StopTime;
                        }
                    }
                }
                else if (autoItem != null) // timingItem == null
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
                else if (timingItem != null) // autoItem == null
                {
                    if (startTime < timingItem.ScheduleInfo.StartTime)
                    {
                        var duration = timingItem.ScheduleInfo.StartTime.Subtract(startTime);
                        PlaybillItem autoPaddingItem = CreateAutoPaddingItem(startTime, duration);
                        result.Add(autoPaddingItem);

                        startTime = autoPaddingItem.ScheduleInfo.StopTime; // breakItem.ScheduleInfo.StartTime;
                    }
                    else
                    {
                        Debug.Assert(startTime == timingItem.ScheduleInfo.StartTime);

                        result.Add(timingItem);
                        startTime = timingItem.ScheduleInfo.StopTime;
                        timingItem = null;
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

            Debug.Assert(buildData.TakeTiming() == null);

            autoItem = buildData.TakeAuto();
            while (autoItem!=null)
            {
                var info = autoItem.ScheduleInfo;
                info = new ScheduleInfo(startTime, TimeSpan.Zero, info.ScheduleMode);
                autoItem.ScheduleInfo = info;

                result.Add(autoItem);

                autoItem = buildData.TakeAuto();
            }
        }
    }
}
