using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace backend.Models
{
  public class CallBack
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("phone")]
    public string Phone { get; set; }

    [BsonElement("calledBack")]
    public Boolean CalledBack { get; set; }

    [BsonIgnore]
    public string Token { get; set; }
  }
}
