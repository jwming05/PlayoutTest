using System;

namespace FCSPlayout
{
    public interface IDateTimeService
    {
        DateTime LocalNow();
        DateTime UtcNow();
    }
}
