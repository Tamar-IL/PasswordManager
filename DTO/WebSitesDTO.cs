using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;



namespace DTO
{
    public class WebSitesDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]

        public string Name { get; set; }
        [BsonElement("baseAddress")]

        public string baseAddress { get; set; }

    }
}
