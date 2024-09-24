
namespace movie_app.Models
{
    public class TicketBooking
    {

            //[BsonId]
           // [BsonRepresentation(BsonType.ObjectId)]
           
            public string? moviename { get; set; }

            public int? no_tickets { get; set; }

            public string? theatre { get; set; }
            public List<string>? seat_no { get; set; }

    }
}
