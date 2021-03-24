using Pure.Common.Commands;
using Pure.Common.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.Common.Models
{
    public class User : IMongoCommon
    {
        public Guid UId { get; set; }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Gender { get; set; }
        public Attachment Avatar { get; set; }

        public string WalletAddress { get; set; }
        public string ShopAddress { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public bool IsMobilePhoneVerified { get; set; }
        public UserRole UserRole { get; set; }

        public List<string> Notifications { get; set; }
        public List<string> LikedPosts { get; set; }
        public List<string> CommentedPosts { get; set; }
        public List<string> PostIds { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDisclaimed { get; set; }
        public Login Login { get; set; }
    }
}
