using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;



namespace DTO
{
    public class WebSitesDTO
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
