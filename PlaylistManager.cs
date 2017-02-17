using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public class PlaylistManager
    {
        private IPlaylist _playlist;

        private TimeSpan _minDuration; // 播放项的最小时长。

        private void AddTiming(DateTime startTime, MarkableMediaSource mediaSource)
        {
            MarkableMediaSource.Validate(mediaSource, _minDuration);


            // 验证操作的合法性。

            ValidateTimeRange(startTime, mediaSource.PlayRange.Duration);


            // 获取插入位置，执行插入。（获取最后一个开始时间小于startTime的项的索引，）
            // 第一个开始时间大于等于startTime的索引（-1表示列表为空或开始时间全部小于startTime）
            int index = _playlist.FindFirstIndex((i)=>!i.IsSkipped() && i.ScheduleInfo.StartTime>=startTime);
            index = index < 0 ? _playlist.Count : index;


            // 触发事件。（决定是否要继续操作）

            SchedulablePlayItem playItem = SchedulablePlayItem.Create(startTime, ScheduleMode.Timing, mediaSource);

            Insert(index, playItem);

            // 重新计算调度信息。

            if (index > 0)
            {
                // 有前部片断
                var prevTimingIndex=_playlist.FindLastIndex(index - 1, i => i.ScheduleInfo.ScheduleMode == ScheduleMode.Timing);

                PlaylistSegmentBuilder segmentBuilder=new PlaylistSegmentBuilder();
                ISchedulablePlayItem beginItem;


                if (prevTimingIndex == -1)
                {
                    beginItem = null;
                    // [0, index-1], 注：index-1可能等于0
                    segmentBuilder.SetTimingRange(beginItem, playItem);
                    for(int i = prevTimingIndex+1/*0*/; i < index; i++)
                    {
                        segmentBuilder.Add(_playlist[i]);
                    }


                    // 替换范围：[0, index-1] (删除的起始索引0==prevTimingIndex+1,数量index==index-prevTimingIndex-1)
                }
                else
                {
                    beginItem = _playlist[prevTimingIndex];
                    segmentBuilder.SetTimingRange(beginItem, playItem);

                    // 注：prevTimingIndex可能等于index-1，这时不会加入任何项。
                    for (int i = prevTimingIndex + 1; i < index; i++)
                    {
                        segmentBuilder.Add(_playlist[i]);
                    }

                    // 当prevTimingIndex小于index-1时。
                    // 替换范围：[prevTimingIndex + 1, index-1] (删除的起始索引prevTimingIndex + 1,数量index-prevTimingIndex-1 )

                    // 当prevTimingIndex等于index-1时。(删除的起始索引prevTimingIndex + 1==index,数量index-prevTimingIndex-1==0 )
                    // 在index处插入0个或1个。
                }


            }

            if (index < _playlist.Count - 1)
            {
                // 有后部片断
                var nextTimingIndex = _playlist.FindFirstIndex(index + 1, i => i.ScheduleInfo.ScheduleMode == ScheduleMode.Timing);
                PlaylistSegmentBuilder segmentBuilder = new PlaylistSegmentBuilder();
                ISchedulablePlayItem endItem;
                if (nextTimingIndex == -1)
                {
                    endItem = null;
                    // [index + 1, _playlist.Count - 1], 注：index + 1可能等于_playlist.Count - 1
                    segmentBuilder.SetTimingRange(playItem, endItem);

                    nextTimingIndex = _playlist.Count;
                    for (int i = index + 1; i < nextTimingIndex/*_playlist.Count*/; i++)
                    {
                        segmentBuilder.Add(_playlist[i]);
                    }

                    // 替换范围：[index + 1, _playlist.Count - 1] (删除的起始索引index + 1, 数量nextTimingIndex-index-1==_playlist.Count-index-1 )
                }
                else
                {
                    endItem = _playlist[nextTimingIndex];
                    segmentBuilder.SetTimingRange(playItem, endItem);

                    // 注：nextTimingIndex可能等于index+1，这时不会加入任何项。
                    for (int i = index + 1; i < nextTimingIndex; i++)
                    {
                        segmentBuilder.Add(_playlist[i]);
                    }

                    // 当nextTimingIndex大于index+1时。
                    // 替换范围：[index+1, nextTimingIndex-1] (删除的起始索引index+1,数量nextTimingIndex-index-1 )

                    // 当nextTimingIndex等于index+1时。(删除的起始索引index+1, 数量nextTimingIndex-index-1==0 )
                    // 在index+1处插入0个或1个。
                }
            }
            
            // 触发事件（表示操作完成）。
        }

        // 确保没有定时冲突。
        private void ValidateTimeRange(DateTime startTime, TimeSpan duration)
        {
            var stopTime = startTime.Add(duration);


            var index = _playlist.FindFirstIndex((i) =>
                (i.ScheduleInfo.ScheduleMode==ScheduleMode.Timing || i.ScheduleInfo.ScheduleMode==ScheduleMode.TimingBreak) && 
                !i.IsSkipped() &&
                //!(i.ScheduleInfo.StopTime<=startTime || i.ScheduleInfo.StartTime>=stopTime)
                (i.ScheduleInfo.StopTime>startTime && i.ScheduleInfo.StartTime<stopTime));

            if (index >= 0)
            {
                var item = _playlist[index];
                throw new InvalidOperationException(string.Format("与{0}'{1}'之间有时间冲突。", 
                    item.ScheduleInfo.ScheduleMode==ScheduleMode.Timing ? "定时播" : "定时插播", item.Title));
            }
        }

        private void Insert(int index, SchedulablePlayItem playItem)
        {
            _playlist.Insert(index, playItem);
            //throw new NotImplementedException();
        }
    }
}
