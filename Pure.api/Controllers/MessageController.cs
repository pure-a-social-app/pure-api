using Microsoft.AspNetCore.Mvc;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pure.api.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("getUserChatList")]
        public async Task<IActionResult> GetUserChatList(string id)
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

                var chatRooms = await _messageService.GetUserChatList(id);

                return Ok(new { Success = true, ChatRooms = chatRooms });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("getChatRoom")]
        public async Task<IActionResult> GetRoomMessages([FromQuery] GetRoomMessageViewModel vm)
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

                var chatRoom = await _messageService.GetRoomMessages(vm);

                return Ok(new { Success = true, ChatRoom = chatRoom });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new post!" });
            }
        }
    }
}
