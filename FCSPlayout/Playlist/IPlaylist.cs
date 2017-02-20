using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public interface IPlaylist
    {
        IPlaylistItem this[int index]{ get; }
        int Count { get; }

        void Insert(int index, IPlaylistItem item);
        void RemoveAt(int startIndex);

        DateTime? MinStartTime { get; }
    }


    
}
