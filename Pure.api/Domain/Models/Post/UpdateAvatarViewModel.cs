using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Post
{
    public class UpdateAvatarViewModel
    {
        public string LoginId { get; set; }
        public string Content { get; set; }
    }
}
