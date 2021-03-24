using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pure.Common.Contracts;
using System;

namespace Pure.Common.Models
{
    public enum MessageType
    {
        Text = 1,
        Attachment = 2,
    }

    public enum AttachmentType
    {
        Image = 1,
    }

    public class ChatMessage : IMongoCommon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public Attachment Attachment { get; set; }

        public string CreatedById { get; set; }
        public DateTime SentOn { get; set; }
        public DateTime ReadOn { get; set; }
        
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class Attachment 
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string AttachmentUrl { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public AttachmentType AttachmentType { get; set; }
    }
}
