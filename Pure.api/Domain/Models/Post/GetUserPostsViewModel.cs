using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Post
{
    public class GetUserPostsViewModel
    {
        public List<Common.Models.Post> Posts { get; set; }
        public bool IsMine { get; set; }
        public string UserName { get; set; }
    }
}
