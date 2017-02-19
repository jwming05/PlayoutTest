using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public class PlayoutSystem
    {
        private IList<IPlayItem> _waittingList;

        private IList<IPlayItem> _completedList;  // 编单器将操作该列表。


        private IPlayItem _currentPlayItem;
        private IPlayItem _nextPlayItem;
        private IList<IPlayItem> _skipedList;


        private void OnCurrentPlayItemStopped()
        {

        }

        private void OnNextPlayItemStarted()
        {
            if (_currentPlayItem != null)
            {
                _completedList.Add(_currentPlayItem);

                if (_skipedList.Count > 0)
                {
                    for (int i = 0; i < _skipedList.Count; i++)
                    {
                        _completedList.Add(_skipedList[i]);
                    }

                    _skipedList.Clear();
                }
            }

            _currentPlayItem = _nextPlayItem;
            _nextPlayItem = null;

            // 移除列表中在current play item前面的所有playItem
            // 如果播放时长不小于某个值，则解锁current play item // ?
        }

        private void OnNextPlayItemChanged()
        {
            // 更新_nextPlayItem
            _nextPlayItem = null;
            // 锁定next play item及列表中在其前面的所有playItem

            var index=_waittingList.IndexOf(_nextPlayItem);
            while (index > 0)
            {
                var temp = _waittingList[0];

                _skipedList.Add(temp);
                _waittingList.RemoveAt(0);
                index--;
            }

            // 移除_nextPlayItem
            _waittingList.RemoveAt(0);
        }


    }
}
