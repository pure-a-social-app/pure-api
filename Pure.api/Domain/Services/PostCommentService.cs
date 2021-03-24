using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Newtonsoft.Json;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Post;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Services
{
    public class PostCommentService : IPostCommentService
    {
        private IRepository<User> _userRepository;
        private IRepository<Post> _postRepository;
        private IFileService _fileService;

        public PostCommentService(IRepository<User> userRepository, IRepository<Post> postRepository,
            IFileService fileService)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _fileService = fileService;
        }

        public async Task<Comment> AddComment(IFormFile photos, string commentDetails)
        {
            var details = JsonConvert.DeserializeObject<AddCommentViewModel>(commentDetails);

            var commenter = (await _userRepository.FindAsync(x => x.Login.Id == details.LoginId)).FirstOrDefault();
            var post = (await _postRepository.FindAsync(x => x.Id == details.PostId)).FirstOrDefault();
            var postOwner = (await _userRepository.FindAsync(x => x.Id == post.UserID)).FirstOrDefault();

            if (post == null || postOwner == null)
                return null;

            Comment comment = new Comment
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Content = details.Content,
                PostId = post.Id,
                CreatedById = commenter.Id,
                CreatedBy = details.CreatedBy,
                Attachment = await _fileService.UploadAttachment(photos),
                CreatedAt = DateTime.UtcNow,
                Avatar = commenter.Avatar != null ? commenter.Avatar : null
            };

            post.Comments.Insert(0, comment);

            await _postRepository.Update(post);
            await _userRepository.Update(postOwner);
                
            if (comment.Avatar != null)
            {
                comment.Avatar.AttachmentUrl = 
                    await _fileService.GetImagePrefix(comment.Avatar.Key, FileType.PostImage);
            }

            commenter.CommentedPosts.Add(comment.Id);
            await _userRepository.Update(commenter);

            return comment;
        }
    }
}
