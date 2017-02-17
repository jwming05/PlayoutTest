using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public interface IPlaylist
    {
        ISchedulablePlayItem this[int index]{ get; }
        int Count { get; }

        void Insert(int index, ISchedulablePlayItem item);
    }


    public static class PlaylistExtensions
    {
        public static int FindLastIndex(this IPlaylist playlist, Func<ISchedulablePlayItem, bool> predicate)
        {
            return playlist.FindLastIndex(playlist.Count - 1, predicate);
        }

        public static int FindLastIndex(this IPlaylist playlist, int startLastIndex, Func<ISchedulablePlayItem, bool> predicate)
        {
            if (startLastIndex >=playlist.Count)
            {
                throw new ArgumentOutOfRangeException("startLastIndex");
            }

            for (int i = startLastIndex; i >= 0; i--)
            {
                if (predicate(playlist[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int FindFirstIndex(this IPlaylist playlist, Func<ISchedulablePlayItem, bool> predicate)
        {
            return playlist.FindFirstIndex(0, predicate);
        }

        public static int FindFirstIndex(this IPlaylist playlist,int startFirstIndex, Func<ISchedulablePlayItem, bool> predicate)
        {
            if (startFirstIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startFirstIndex");
            }

            for (int i = startFirstIndex; i < playlist.Count; i++)
            {
                if (predicate(playlist[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
