using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Models
{
    public class Pagination
    {
        public int Total { get; set; }
        public int PageSize { get; set; }
        public int Current { get; set; }
    }
}
