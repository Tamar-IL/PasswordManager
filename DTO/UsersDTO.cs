using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DTO
{
    public class UsersDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UaerName")]
        public string UaerName { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("email")]
        public string email { get; set; }

        [BsonElement("phone")]
        public string phone { get; set; }
  

        
    }
}
