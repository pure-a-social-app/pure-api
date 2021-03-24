using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Post;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pure.api.Domain.Services
{
    public class PostService : IPostService
    {
        private IRepository<User> _userRepository;
        private IRepository<Post> _postRepository;
        private IFileService _fileService;

        public PostService(IRepository<User> userRepository, IRepository<Post> postRepository,
            IFileService fileService)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _fileService = fileService;
        }

        public async Task<Post> CreatePost(IList<IFormFile> photos, string postDetails)
        {
            try
            {
                var details = JsonConvert.DeserializeObject<CreatePostViewModel>(postDetails);

                var user = (await _userRepository.FindAsync(x => x.Login.Id == details.LoginId)).FirstOrDefault();

                if (user == null)
                    return null;

                Post newPost = new Post
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UId = Guid.NewGuid(),
                    UserID = user.Id,
                    CreatedBy = user.UserName,
                    WalletAddress = user.WalletAddress,
                    Content = details.Content,
                    CreatedAt = DateTime.UtcNow,
                    Avatar = user.Avatar,
                    Attachments = new List<Attachment>(),
                    Comments = new List<Comment>(),
                    LikeUserIds = new List<string>(),
                    IsDeleted = false,
                    IsAvatar = false,
                };

                if (photos != null)
                {
                    foreach (var photo in photos)
                    {
                        var attachment = await _fileService.UploadAttachment(photo);
                        newPost.Attachments.Add(attachment);
                    }
                }

                await _postRepository.Add(newPost);

                user.PostIds.Add(newPost.Id);
                await _userRepository.Update(user);

                return await GetPostImageURL(newPost);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<Post> HandleLike(string loginId, string postId)
        {
            var post = (await _postRepository.FindAsync(x => x.Id == postId)).FirstOrDefault();
            var user = (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault();
           
            var isLiked = false;

            if (!post.LikeUserIds.Contains(user.Id))
            {
                post.LikeUserIds.Add(user.Id);
                isLiked = true;
            }
            else
            {
                post.LikeUserIds.Remove(user.Id);
            }

            await _postRepository.Update(post);
            
            user.LikedPosts.Add(postId);
            await _userRepository.Update(user);

            post.IsLiked = isLiked;

            return post;
        }

        public async Task<List<Post>> GetAllPosts(string loginId)
        {
            try
            {
                var user = (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault();

                if (user == null)
                    return null;

                List<Post> posts = await _postRepository.FindAllAsyncReversed();
                posts.ForEach(x => x.IsLiked = x.LikeUserIds.Contains(user.Id));

                return await GetPostImageURLs(posts);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<GetUserPostsViewModel> GetAllPostsFromUser(string loginId, string friendId)
        {
            try
            {
                var user = (await _userRepository.FindAsync(x => x.Id == friendId)).FirstOrDefault();
                List<Post> posts;

                bool isMine = false;
                if (user.Login.Id == loginId)
                {
                    posts = (await _postRepository.FindAsync(x => x.UserID == user.Id)).ToList();
                    isMine = true;
                }
                else if (user != null)
                {
                    // In the future can filter only me posts
                    posts = (await _postRepository.FindAsync(x => x.UserID == user.Id)).ToList();
                }
                else
                {
                    throw new ApplicationException("Cannot find user by ID"); 
                }
                
                posts.Reverse();
                posts.ForEach(x => x.IsLiked = x.LikeUserIds.Contains(user.Id));

                var reversedPosts = await GetPostImageURLs(posts);

                return new GetUserPostsViewModel { Posts = reversedPosts, IsMine = isMine, UserName = user.UserName };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<GetUserPostsViewModel> GetAllPostsFromUser(string loginId)
        {
            try
            {
                var user = (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault();

                if (user == null)
                {
                    throw new ApplicationException("Cannot find user by ID");
                }

                List<Post> reversedPosts = (await _postRepository.FindAsync(x => x.UserID == user.Id)).ToList();
                reversedPosts.Reverse();
                reversedPosts.ForEach(x => x.IsLiked = x.LikeUserIds.Contains(user.Id));

                    var posts = await GetPostImageURLs(reversedPosts);

                return new GetUserPostsViewModel { Posts = posts, IsMine = true, UserName = user.UserName };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        //----------------------------------------- Avatar ------------------------------------

        public async Task<GetUserPostsViewModel> UpdateAvatar(IFormFile photo, string details)
        {
            try
            {
                var postDetails = JsonConvert.DeserializeObject<UpdateAvatarViewModel>(details);

                var user = (await _userRepository.FindAsync(x => x.Login.Id == postDetails.LoginId)).FirstOrDefault();
                if (user == null)
                {
                    return null;
                }

                var avatarAttachment = await _fileService.UploadAttachment(photo);
                Post newPost = new Post
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UId = Guid.NewGuid(),
                    UserID = user.Id,
                    CreatedBy = user.UserName,
                    WalletAddress = user.WalletAddress,
                    Content = user.UserName + " has just updated a new avatar!",
                    CreatedAt = DateTime.UtcNow,
                    Attachments = new List<Attachment>(),
                    Comments = new List<Comment>(),
                    LikeUserIds = new List<string>(),
                    Avatar = avatarAttachment,
                    IsDeleted = false,
                    IsAvatar = true
                };

                newPost.Attachments.Add(avatarAttachment);
                newPost.Avatar = avatarAttachment;
                await _postRepository.Add(newPost);

                user.Avatar = avatarAttachment;
                user.PostIds.Add(newPost.Id);

                await _userRepository.Update(user);
                await UpdatePostAvatar(user);

                return await GetAllPostsFromUser(user.Login.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<Attachment> GetUserAvatar(string loginId)
        {
            return (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault().Avatar;
        }

        public async Task UpdatePostAvatar(User user)
        {
            var posts = (await _postRepository.FindAsync(x => x.UserID == user.Id)).ToList();
            posts.ForEach(x => x.Avatar = user.Avatar);
            await _postRepository.UpdateMany(posts);

            var commentPosts = (await _postRepository.FindAsync(x => x.Comments.Any(c => c.CreatedById == user.Id))).ToList();
            commentPosts.ForEach(x => x.Comments.ForEach(c => 
            { 
                if (c.CreatedById == user.Id) 
                { 
                    c.Avatar = user.Avatar; 
                } 
            }));
            await _postRepository.UpdateMany(commentPosts);
        }

        //--------------------------------------- Image URL ------------------------------------

        public async Task<Post> GetPostImageURL(Post post)
        {
            var keys = post.Attachments.Select(x => x.Key).ToList();
            var imagePrefixes = await _fileService.GetPostImagePrefixes(keys);
            for (int i = 0; i < imagePrefixes.Count; i++)
            {
                post.Attachments[i].AttachmentUrl = imagePrefixes[i];
            }

            if (post.Avatar != null)
            {
                post.Avatar.AttachmentUrl = await _fileService.GetImagePrefix(post.Avatar.Key, FileType.PostImage);
            }

            for (int i = 0; i < post.Comments.Count; i++)
            {
                if (post.Comments[i].Avatar != null)
                {
                    post.Comments[i].Avatar.AttachmentUrl = 
                        await _fileService.GetImagePrefix(post.Comments[i].Avatar.Key, FileType.PostImage);
                }
            }

            return post;
        }

        public async Task<List<Post>> GetPostImageURLs(List<Post> posts)
        {
            if (posts == null)
                return null;

            foreach (var post in posts)
            {
                await GetPostImageURL(post);
            }

            return posts;
        }
    }
}
