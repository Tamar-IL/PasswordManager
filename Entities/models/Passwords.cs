using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities.models
{
    public class Passwords
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("SiteId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SiteId { get; set; }

        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }



        [BsonElement("DateReg")]
        public string DateReg { get; set; }

        [BsonElement("LastDateUse")]
        public string LastDateUse { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }
    }
}
