using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pure.Common.Contracts;
using System.Collections.Generic;

namespace Pure.Common.Models
{
    public class ChatRoom : IMongoCommon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public string CreatorAvatarUrl { get; set; }

        public string FriendName { get; set; }
        public string FriendId { get; set; }
        public string FriendWalletAddress { get; set; }
        public string FriendAvatarUrl { get; set; }

        public List<ChatMessage> Messages { get; set; }
        public string IsLastRead { get; set; }
        public string IsOnline { get; set; }
        public bool IsDeleted { get; set; }
    }
}