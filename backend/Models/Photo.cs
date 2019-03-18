using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class Photo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("url")]
        public string Url { get; set; }

        [BsonElement("alt")]
        public string Alt { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }
    }
}
