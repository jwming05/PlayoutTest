using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public interface IPlayer:IDisposable
    {
        IPlayItem PlayItem { get; }

        void Start();
        void Stop();

        event EventHandler Started;
        event EventHandler Stopped;
    }
}