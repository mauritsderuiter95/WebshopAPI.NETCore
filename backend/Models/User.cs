using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        [BsonElement("UserName")]
        public string Username { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Token")]
        public string Token { get; set; }

        [BsonElement("UserRoles")]
        public string Role { get; set; }

        [BsonElement("TokenExpires")]
        public DateTime Expires { get; set; }

        [BsonElement("Adresses")]
        public List<Address> Adresses { get; set; }
    }
}
