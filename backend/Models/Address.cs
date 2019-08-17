using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class Address
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Company")]
        public string Company { get; set; }

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        [BsonElement("Street")]
        public string Street { get; set; }

        [BsonElement("Street2")]
        public string Street2 { get; set; }

        [BsonElement("Number")]
        public string Number { get; set; }

        [BsonElement("Postalcode")]
        public string Postalcode { get; set; }

        [BsonElement("City")]
        public string City { get; set; }

        [BsonElement("Notitions")]
        public string Notitions { get; set; }
    }
}
