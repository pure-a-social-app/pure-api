using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Search
{
    public class SearchPeopleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string WalletAddress { get; set; }
        public Attachment Avatar { get; set; }
    }
}
