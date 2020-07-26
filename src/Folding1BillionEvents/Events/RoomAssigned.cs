using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folding1BillionEvents.Events
{
    public struct RoomAssigned : EventBase
    {
        public string RoomId { get; set; }
        public string RoomName { get; set; }
    }
}
