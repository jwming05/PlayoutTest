using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public class PlaylistManager
    {
        private IPlaylist _playlist;


        // 该属性放在这还是放在IPlaylist接口中？
        public PlaylistConfiguration Configuration { get; private set; }
        private PlaylistSegmentBuilder Build()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取定时播的插入位置。
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private int FindTimingInsertIndex(DateTime startTime)
        {
            // 获取插入位置，执行插入。
            // 第一个开始时间大于等于startTime的索引（-1表示列表为空或开始时间全部小于startTime）
            int index = _playlist.FindFirstIndex((i) => !i.IsSkipped() && i.ScheduleInfo.StartTime >= startTime);

            // index==-1是在尾部插入（包含_playlist为空的情况）。
            index = index < 0 ? _playlist.Count : index;
            return index;
        }

        private void AddTiming(DateTime startTime, IPlaySource playSource)
        {
            // 验证操作的合法性。
            ValidatePlayDuration(playSource.PlayRange.Duration);
            ValidateStartTime(startTime, false);
            EnsureNoTimingConflict(startTime, playSource.PlayRange.Duration);


            // 获取插入位置。
            // 第一个开始时间大于等于startTime的索引（-1表示列表为空或开始时间全部小于startTime）
            int index = _playlist.FindFirstIndex((i)=>!i.IsSkipped() && i.ScheduleInfo.StartTime>=startTime);
            index = index < 0 ? _playlist.Count : index;

            // 下面的处理代码确保顺播片断不会被定时播分隔。
            var tempIndex = index;
            while (tempIndex < _playlist.Count)
            {
                var item = _playlist[tempIndex];
                if (item.ScheduleInfo.ScheduleMode == ScheduleMode.Timing)
                {
                    break;
                }
                else if (item.ScheduleInfo.ScheduleMode == ScheduleMode.TimingBreak)
                {
                    continue;
                }
                else if (!item.IsSegment() || item.IsFirstSegment())
                {
                    break;
                }
                else
                {
                    if (tempIndex != index)
                    {
                        var temp = _playlist[tempIndex];
                        _playlist.RemoveAt(tempIndex);

                        _playlist.Insert(index, temp);
                    }
                    index++;
                }
                tempIndex++;
            }

            // 触发事件前置事件。

            PlaybillItem playItem = PlaybillItem.CreateTiming(startTime,new NormalPlaySource(playSource),false);

            Insert(index, playItem);

            // 重新计算调度信息。
            DateTime beginTime;
            DateTime endTime;
            if (index == 0)
            {
                if(_playlist.HasMinStartTime() && _playlist.MinStartTime.Value < startTime)
                {
                    beginTime = _playlist.MinStartTime.Value;
                    endTime = startTime;

                    using (var builder = Build())
                    {
                        Replace(0, 0, builder.Build(beginTime, endTime));
                    }
                }
            }
            else if (index > 0) // 有前部片断
            {    
                var prevIndex = _playlist.FindLastIndex(index - 1, i => !i.IsSkipped() && i.ScheduleInfo.StopTime<=startTime);
                endTime = startTime;
                if (prevIndex == -1)
                {
                    beginTime = _playlist[0].ScheduleInfo.StartTime; // startTime;
                    beginTime = beginTime > endTime ? endTime : beginTime;


                    // [prevIndex+1, index-1], 注：index-prevIndex+1
                    // 替换范围：[prevIndex+1, index-1] (删除的起始索引0==prevIndex+1,数量index==index-prevIndex-1)
                }
                else
                {
                    beginTime = _playlist[prevIndex].ScheduleInfo.StopTime;
                    // 注：prevIndex可能等于index-1。
                    // 当prevIndex小于index-1时。
                    // 替换范围：[prevIndex + 1, index-1] (删除的起始索引prevIndex + 1,数量index-prevIndex-1 )

                    // 当prevIndex等于index-1时。(删除的起始索引prevIndex + 1==index,数量index-prevIndex-1==0 )
                    // 在index处插入0个或1个。
                }

                using (var builder = Build())
                {
                    for (int i = prevIndex + 1; i < index; i++)
                    {
                        builder.Add(_playlist[i].PlaybillItem);
                    }

                    Replace(prevIndex + 1, index - prevIndex - 1, builder.Build(beginTime,endTime));
                }
            }

            if (index < _playlist.Count - 1) // 有后部片断
            {
                var nextIndex = _playlist.FindFirstIndex(index + 1, i => !i.IsSkipped() && i.ScheduleInfo.ScheduleMode==ScheduleMode.Timing);
                beginTime = playItem.ScheduleInfo.StopTime;
                if (nextIndex == -1)
                {        
                    endTime = DateTime.MaxValue;
                    nextIndex = _playlist.Count;

                    // [index + 1, _playlist.Count - 1], 注：index + 1可能等于_playlist.Count - 1
                    // 替换范围：[index + 1, _playlist.Count - 1] (删除的起始索引index + 1, 数量nextIndex-index-1==_playlist.Count-index-1)
                }
                else
                {
                    endTime = _playlist[nextIndex].ScheduleInfo.StartTime;

                    // 注：nextIndex可能等于index+1。
                    // 当nextIndex大于index+1时。
                    // 替换范围：[index+1, nextIndex-1] (删除的起始索引index+1,数量nextIndex-index-1 )

                    // 当nextIndex等于index+1时。(删除的起始索引index+1, 数量nextIndex-index-1==0 )
                    // 在index+1处插入0个或1个。          
                }

                using (var builder = Build())
                {
                    for (int i = index + 1; i < nextIndex; i++)
                    {
                        builder.Add(_playlist[i].PlaybillItem);
                    }
                    Replace(index + 1, nextIndex - index - 1, builder.Build(beginTime,endTime));
                }
            }
            
            // 触发事件（表示操作完成）。
        }

        private void ValidateStartTime(DateTime startTime, bool isBreak)
        {
            if (_playlist.HasMinStartTime() && startTime < _playlist.MinStartTime.Value)
            {
                throw new ArgumentOutOfRangeException("startTime",
                        string.Format("{0}的开始时间无效<{1}>，必须大于等于{2}。", isBreak ? "定时插播" : "定时播",
                        startTime, _playlist.MinStartTime.Value));

                // TODO: 是否需要其他验证？
            }
        }

        private void ValidatePlayDuration(TimeSpan duration)
        {
            
        }

        private void Replace(int startIndex, int deleteCount, IList<PlaybillItem> insertList)
        {
            while (deleteCount > 0)
            {
                _playlist.RemoveAt(startIndex);
                deleteCount--;
            }

            for(int i = insertList.Count - 1; i >= 0; i--)
            {
                _playlist.Insert(startIndex, insertList[i]);
            }
        }

        // 确保没有定时冲突。
        private void EnsureNoTimingConflict(DateTime startTime, TimeSpan duration)
        {
            _playlist.EnsureNoTimingConflict(startTime, duration);
        }

        private void Insert(int index, IPlaylistItem playItem)
        {
            _playlist.Insert(index, playItem);
        }
    }
}
