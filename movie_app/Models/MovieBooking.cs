
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace movie_app.Models
{
    public class MovieBooking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? moviename { get; set; }

        public int? no_tickets { get; set; }
        
        public string? theatre { get; set; }
        public List<string>? seatsBooked { get;set; }
    }
}
