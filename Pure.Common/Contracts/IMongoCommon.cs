using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Contracts
{
    public interface IMongoCommon : IMongoEntity<string>
    {
        public bool IsDeleted { get; set; }
    }
}
