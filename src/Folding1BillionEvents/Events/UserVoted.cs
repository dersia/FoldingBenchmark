﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folding1BillionEvents.Events
{
    public struct UserVoted : EventBase
    {
        public string UserId { get; set; }
    }
}
