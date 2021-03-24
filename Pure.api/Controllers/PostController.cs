using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Post;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pure.api.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IPostCommentService _postCommentService;

        public PostController(IPostService postService, IPostCommentService postCommentService)
        {
            _postService = postService;
            _postCommentService = postCommentService;
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromForm] IList<IFormFile> photos,
            [FromForm] string details)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values
                        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                var post = await _postService.CreatePost(photos, details);

                if (post != null)
                    return Ok(new { Success = true, Post = post });
                else
                    return BadRequest(new { Message = "Try block: Cannot create new post!" });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("getUserPosts")]
        public async Task<IActionResult> GetUserPosts([FromQuery]string loginId, string friendId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values
                        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                GetUserPostsViewModel posts;

                if (string.IsNullOrEmpty(friendId))
                {
                    posts = await _postService.GetAllPostsFromUser(loginId);
                }
                else
                {
                    posts = await _postService.GetAllPostsFromUser(loginId, friendId);
                }

                return Ok(new { Success = true, Posts = posts });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("getAllPosts")]
        public async Task<IActionResult> GetAllPosts(string loginId)
        {
            try
            {
                var posts = await _postService.GetAllPosts(loginId);

                return Ok(new { Success = true, Posts = posts });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("updateAvatar")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile photo,
            [FromForm] string details)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values
                        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                var posts = await _postService.UpdateAvatar(photo, details);

                if (posts != null)
                    return Ok(new { Success = true, Posts = posts });
                else
                    return BadRequest(new { Message = "Try block: Cannot create new post!" });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }

        //-------------------------------- Like ----------------------------------------

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("handleLike")]
        public async Task<IActionResult> HandleLike([FromQuery] string loginId, [FromQuery] string postId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values
                        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                var post = await _postService.HandleLike(loginId, postId);

                if (post != null)
                    return Ok(new { Success = true, Post = post });
                else
                    return BadRequest(new { Message = "Try block: Cannot create new post!" });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }

        //-------------------------------- Comment ----------------------------------------

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("addComment")]
        public async Task<IActionResult> AddComment([FromForm] IFormFile photo, [FromForm] string details)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values
                        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                var comment = await _postCommentService.AddComment(photo, details);

                if (comment != null)
                    return Ok(new { Success = true, Comment = comment });
                else
                    return BadRequest(new { Message = "Try block: Cannot create new post!" });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }
    }
}
