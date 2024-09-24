using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace movie_app.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("LoginId")]
        public string LoginId { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("ContactNumber")]
        public string ContactNumber { get; set; }
    }
}