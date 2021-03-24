using Microsoft.AspNetCore.Http;
using Pure.api.Domain.Models.Post;
using Pure.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface IPostService
    {
        Task<Post> CreatePost(IList<IFormFile> photos, string details);
        Task<Post> HandleLike(string loginId, string postId);
        Task<GetUserPostsViewModel> GetAllPostsFromUser(string loginId, string friendId);
        Task<GetUserPostsViewModel> GetAllPostsFromUser(string loginId);
        Task<List<Post>> GetAllPosts(string loginId);
        Task<GetUserPostsViewModel> UpdateAvatar(IFormFile photo, string details);
        Task<Attachment> GetUserAvatar(string loginId);
        Task UpdatePostAvatar(User user);
    }
}
