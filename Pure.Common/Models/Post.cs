using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pure.Common.Contracts;
using System;
using System.Collections.Generic;

namespace Pure.Common.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public string Content { get; set; }

        public string CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Attachment Avatar { get; set; }

        public Attachment Attachment { get; set; }
    }
        
    public class Post : IMongoCommon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Guid UId { get; set; }
        public string UserID { get; set; }
        public string WalletAddress { get; set; }

        public Attachment Avatar { get; set; }
        public string Content { get; set; }
        public bool IsAvatar { get; set; }
        public bool IsLiked { get; set; }

        public List<Attachment> Attachments { get; set; }
        public List<Comment> Comments { get; set; }
        public List<string> LikeUserIds { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        // These variables for video purpose only
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
