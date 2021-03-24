using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pure.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Models
{
    public enum Category
    {
        Clothes = 0,
        Beauty
    }

    public class ShoppingItem : IMongoCommon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Guid UId { get; set; }
        public string SellerUserId { get; set; }
        public string ShopAddress { get; set; }

        public string Name { get; set; }
        public double Price { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool IsShippable { get; set; }
        public List<Attachment> Attachments { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public Category Catergory { get; set; }
        public uint Stock { get; set; }

        public bool IsSold { get; set; }
        public string BuyerUserId { get; set; }
        public string ShippingLocation { get; set; }
        public bool IsReceived { get; set; }
    }
}
