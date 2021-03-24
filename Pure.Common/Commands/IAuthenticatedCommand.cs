using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Commands
{
    public interface IAuthenticatedCommand : ICommand
    {
        Guid PersonId { get; set; }
    }
}
