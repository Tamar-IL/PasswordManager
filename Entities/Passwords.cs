using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities
{
    public class Passwords
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        
        [BsonElement("PasswordId")]
        public int PasswordId { get; set; }

        [BsonElement("UserId")]
        public int UserId { get; set; }

        [BsonElement("SiteId")]
        public int SiteId { get; set; }

        [BsonElement("DateReg")]
        public string DateReg { get; set; }

        [BsonElement("LastDateUse")]
        public string LastDateUse { get; set; }
    }
}
