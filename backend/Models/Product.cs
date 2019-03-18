using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using backend.Models;

namespace backend.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string ProductName { get; set; }

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("Category")]
        public int Category { get; set; }

        [BsonElement("Discount")]
        public decimal Discount { get; set; }

        [BsonElement("ShortDescription")]
        public string ShortDescription { get; set; }

        [BsonElement("LongDescription")]
        public string LongDescription { get; set; }

        [BsonElement("Photos")]
        public List<Photo> Photos { get; set; }
    }
}
