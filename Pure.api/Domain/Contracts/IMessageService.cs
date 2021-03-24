using Pure.api.Domain.Models.Message;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface IMessageService
    {
        Task<ChatRoom> GetRoomMessages(GetRoomMessageViewModel vm);
        Task<List<UserChatRoom>> GetUserChatList(string loginId);
        Task<ChatRoom> GetChatRoomFromId(string userId, string friendId);
    }
}
