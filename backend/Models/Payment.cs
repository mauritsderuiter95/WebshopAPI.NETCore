using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PaymentId { get; set; }

        [BsonElement("resource")]
        public string Resource { get; set; }

        [BsonElement("mollieid")]
        public string Id { get; set; }

        [BsonElement("mode")]
        public string Mode { get; set; }

        [BsonElement("createdAt")]
        public DateTime createdAt { get; set; }

        [BsonElement("amount")]
        public AmountClass Amount { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("method")]
        public string Method { get; set; }

        [BsonElement("metadata")]
        public MetadataClass Metadata { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("isCancelable")]
        public string IsCancelable { get; set; }

        [BsonElement("expiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [BsonElement("detail")]
        public string Detail { get; set; }

        [BsonElement("profileId")]
        public string ProfileId { get; set; }

        [BsonElement("sequenceType")]
        public string SequenceType { get; set; }

        [BsonElement("redirectUrl")]
        public string RedirectUrl { get; set; }

        [BsonElement("webhookUrl")]
        public string WebhookoUrl { get; set; }

        [BsonElement("_links")]
        public Links _links { get; set; }

        public class Links
        {
            public Link Self { get; set; }
            public Link Checkout { get; set; }
            public Link documentation { get; set; }
        }

        public class Link
        {
            public string Href { get; set; }
            public string Type { get; set; }
        }

        public class AmountClass
        {
            public string Value { get; set; }
            public string Currency { get; set; }
        }

        public class MetadataClass
        {
            public string Order_id { get; set; }
        }
    }
}
