using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public interface IPlaybillItem
    {
        ScheduleInfo ScheduleInfo { get; set; }

        ScheduleMode ScheduleMode { get; }
        string Title { get; }

        //MarkableMediaSource MarkableSource { get; }
        IPlaySource PlaySource { get; }

        bool CanMerge(IPlaybillItem nextItem);
        IPlaybillItem Merge(IPlaybillItem nextItem);
        void Split(TimeSpan duration, out IPlaybillItem first, out IPlaybillItem second);
    }
}
