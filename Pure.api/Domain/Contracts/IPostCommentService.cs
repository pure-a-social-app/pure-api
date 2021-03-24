using Microsoft.AspNetCore.Http;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface IPostCommentService
    {
        Task<Comment> AddComment(IFormFile photos, string details);
    }
}
