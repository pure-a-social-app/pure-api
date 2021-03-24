using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Events
{
    public interface IAuthenticatedEvent : IEvent
    {
        Guid PersonId { get; set; }
    }
}
