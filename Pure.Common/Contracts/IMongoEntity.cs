using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Contracts
{
    public interface IMongoEntity<TId>
    {
        TId Id { get; set; }
    }
}
