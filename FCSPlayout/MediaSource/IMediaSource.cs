using System;

namespace FCSPlayout
{
    public interface IMediaSource
    {
        string Title { get; }
        TimeSpan? NativeDuration { get; }

        //PlayRange Adjust(PlayRange range);
    }
}
