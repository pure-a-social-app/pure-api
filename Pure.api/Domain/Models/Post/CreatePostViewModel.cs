using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Post
{
    public class CreatePostViewModel
    {
        public string LoginId { get; set; }
        public string Content { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    public class AddCommentViewModel
    {
        public string LoginId { get; set; }
        public string PostId { get; set; }
        public string CreatedBy { get; set; }
        public string Content { get; set; }
    }
}
