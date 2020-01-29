using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace backend.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("orderNumber")]
        public int Ordernumber { get; set; }

        [BsonElement("cartId")]
        public int CartId { get; set; }

        [BsonElement("products")]
        public List<CartProduct> Products { get; set; }

        [BsonElement("ideal")]
        public bool Ideal { get; set; }

        [BsonElement("orderPayment")]
        public Payment orderPayment { get; set; }

        [BsonElement("sendingCosts")]
        public decimal SendingCosts { get; set; }

        [BsonElement("user")]
        public User User { get; set; }

        [BsonElement("created")]
        public DateTime Created { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }
        
        [BsonElement("key")]
        public string Key { get; set; }
    }
}
