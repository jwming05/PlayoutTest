using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public interface IPlaySource
    {
        string Title { get; }
        PlayRange PlayRange { get; }
        //MarkableMediaSource MarkableSource { get; }
        IMediaSource MediaSource { get; }
    }
}
