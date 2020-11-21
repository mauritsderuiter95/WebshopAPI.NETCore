using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace backend.Models
{
    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("machine")]
        public Machine Machine { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonIgnore]
        public string Token { get; set; }
    }
}
