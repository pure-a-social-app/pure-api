using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pure.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Models
{
    public class ShoppingSeller : IMongoCommon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Guid UId { get; set; }
        public string UserId { get; set; } 

        public string ShopAddress { get; set; }
        public List<ShoppingItem> ShoppingItems { get; set; }
        public bool IsDeleted { get; set; }

        //public ShoppingSeller(Guid uId, string loginId)
        //{
        //    Id = ObjectId.GenerateNewId().ToString();
        //    UId = uId;
        //    LoginId = loginId;
        //    ShoppingItems = new List<ShoppingItem>();
        //    IsDeleted = false;
        //}
    }
}
