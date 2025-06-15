using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DTO
{
    public class UsersDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserName")]
        public string UserName { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }
        [BsonElement("VP")]
        public string VP { get; set; }
  

        
    }
}
