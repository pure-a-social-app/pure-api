using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Message
{
    public class UserChatRoom
    {
        public string ChatRoomId { get; set; }
        public string LastMessage { get; set; }
        
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Attachment Avatar { get; set; }
        public string UserWalletAddress { get; set; }
    }
}
