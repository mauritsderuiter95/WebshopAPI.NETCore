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

        [BsonElement("Company")]
        public String Company { get; set; }

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

        //[BsonElement("Adress")]
        //public List<Address> Adressses { get; set; }

        [BsonElement("Street")]
        public string Street { get; set; }

        [BsonElement("Street2")]
        public string Street2 { get; set; }

        [BsonElement("Zipcode")]
        public string Zipcode { get; set; }

        [BsonElement("City")]
        public string City { get; set; }
    }
}
