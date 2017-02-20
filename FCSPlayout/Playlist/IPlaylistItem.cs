using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public interface IPlaylistItem
    {
        string Title { get; }

        ScheduleInfo ScheduleInfo { get; set; }

        PlaybillItem PlaybillItem { get; }
    }
}
