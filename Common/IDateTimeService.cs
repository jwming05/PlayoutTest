using System;

namespace PlayoutTest
{
    public interface IDateTimeService
    {
        DateTime LocalNow();
        DateTime UtcNow();
    }
}
