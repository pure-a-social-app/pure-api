using System;
using System.Collections.Generic;
using Pure.Common.Models;

namespace Pure.api.Domain.Models.Message
{
    public class NewMessageViewModel
    {
        public List<ChatMessage> ChatMessages { get; set; }
        public string LoginId { get; set; }
        public string ChatRoomId { get; set; }
    }
}
