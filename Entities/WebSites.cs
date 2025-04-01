using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class WebSites
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("SiteId")]

        public int SiteId { get; set; }

        [BsonElement("name")]

        public string Name { get; set; }
        [BsonElement("baseAddress")]

        public string baseAddress { get; set; }
    }
}
