using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace Entities.models
{
    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("token")]
        public string Token { get; set; } = string.Empty;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("ipAddress")]
        public string? IpAddress { get; set; }

        [BsonElement("userAgent")]
        public string? UserAgent { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("revokedAt")]
        public DateTime? RevokedAt { get; set; }

        [BsonElement("revokeReason")]
        public string? RevokeReason { get; set; }

        [BsonIgnore]
        public bool IsValid => IsActive && DateTime.UtcNow < ExpiresAt && RevokedAt == null;

        public void Revoke(string reason = "התנתקות ידנית")
        {
            IsActive = false;
            RevokedAt = DateTime.UtcNow;
            RevokeReason = reason;
        }
    }
}