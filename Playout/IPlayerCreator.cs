﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public interface IPlayerCreator
    {
        IPlayer Create(IPlayItem playItem); 
    }
}
