using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Type")]
        public List<string> Type { get; set; }

        [BsonElement("MachineName")]
        public string MachineName { get; set; }

        [BsonElement("Photo")]
        public Photo Photo { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }

        [BsonElement("Price")]
        public decimal Price { get; set; }

        public List<KeyValuePair<string, string>> Attributes { get; set; }

        public string Content { get; set; }

        public int productCount { get; set; }

        public int userCount { get; set; }
    }
}
