using movie_app.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace movie_app.Services
{
    public class MongoDBServices
    {
        private readonly IMongoCollection<MovieBooking> _movieCollection;
        private readonly IMongoCollection<TicketBooking> _ticketCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public MongoDBServices(IOptions<MongoDBSettings> MongoDBSettings)
        {
            Console.WriteLine(MongoDBSettings.Value);
            var mongoClient = new MongoClient(
                MongoDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                MongoDBSettings.Value.DatabaseName);

            _movieCollection = mongoDatabase.GetCollection<MovieBooking>(
                MongoDBSettings.Value.CollectionName);

            _ticketCollection = mongoDatabase.GetCollection<TicketBooking>(
                MongoDBSettings.Value.CollectionName2);

            _usersCollection = mongoDatabase.GetCollection<User>(
                MongoDBSettings.Value.CollectionName3);
        }

        public async Task<List<MovieBooking>> GetAsync() =>
            await _movieCollection.Find(_ => true).ToListAsync();

        public async Task<List<MovieBooking>> GetAsync(string moviename) =>
            await _movieCollection.Find(x => x.moviename.ToLower() == moviename).ToListAsync();


        public async Task CreateAsync(MovieBooking newMovie) =>
            await _movieCollection.InsertOneAsync(newMovie);
        public async Task<bool> IsMovieInTheaterAsync(string movieName, string theaterName)
        {
            // Create a filter to check for the movie in the specified theater
            var filter = Builders<MovieBooking>.Filter.And(
                Builders<MovieBooking>.Filter.Eq(m => m.moviename, movieName),
                Builders<MovieBooking>.Filter.Eq(m => m.theatre, theaterName)
            );

            var movie = await _movieCollection.Find(filter).FirstOrDefaultAsync();
            return movie != null; // Returns true if the movie is found in the specified theater
        }
        public async Task<bool> UpdateAvailableTicketsAsync(string movieName, string theaterName, int? ticketsBooked, List<string>? seat)
        {
            var filter = Builders<MovieBooking>.Filter.And(
                Builders<MovieBooking>.Filter.Eq(m => m.moviename, movieName),
                Builders<MovieBooking>.Filter.Eq(m => m.theatre, theaterName)
            );

            var update = Builders<MovieBooking>.Update.Combine(
                Builders<MovieBooking>.Update.Inc(m => m.no_tickets, -ticketsBooked),
                Builders<MovieBooking>.Update.AddToSetEach(m => m.seatsBooked,seat)
    );


            var result = await _movieCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        public async Task UpdateAsync(string id, MovieBooking updatedMovie) =>
            await _movieCollection.ReplaceOneAsync(x => x.Id == id, updatedMovie);

        public async Task RemoveAsync(string id) =>
            await _movieCollection.DeleteOneAsync(x => x.Id == id);

        public async Task UpdateMovieAsync(string movieName, string theaterName, int? seatsToBook,List<string> seat)
        {
            var filter = Builders<MovieBooking>.Filter.And(
                Builders<MovieBooking>.Filter.Eq(m => m.moviename, movieName),
                Builders<MovieBooking>.Filter.Eq(m => m.theatre, theaterName)
            );
            
            var update = Builders<MovieBooking>.Update.Combine(
                Builders<MovieBooking>.Update.Inc(m => m.no_tickets, -seatsToBook),
                Builders<MovieBooking>.Update.AddToSetEach(m => m.seatsBooked, seat));
            await _movieCollection.UpdateOneAsync(filter, update);

           
        }
        public async Task UpdateTicketAsync(TicketBooking newTicket)
        {
            var filter = Builders<TicketBooking>.Filter.And(
                Builders<TicketBooking>.Filter.Eq(m => m.moviename, newTicket.moviename),
                Builders<TicketBooking>.Filter.Eq(m => m.theatre, newTicket.theatre)
            );

            var update = Builders<TicketBooking>.Update.Combine(
                Builders<TicketBooking>.Update.Set(t => t.no_tickets, newTicket.no_tickets),
                Builders<TicketBooking>.Update.Set(t => t.seat_no, newTicket.seat_no)
            );

            await _ticketCollection.UpdateOneAsync(filter, update);
        }
        public async Task DeleteMovieAsync(string movieName)
        {
            // Delete the movie from the movies collection
            var movieFilter = Builders<MovieBooking>.Filter.Eq(m => m.moviename, movieName);
            await _movieCollection.DeleteOneAsync(movieFilter);

            // Delete related tickets from the tickets collection
            var ticketFilter = Builders<TicketBooking>.Filter.Eq(t => t.moviename, movieName);
            await _ticketCollection.DeleteManyAsync(ticketFilter);
        }

        public async Task CreateAsync(TicketBooking newticket) =>
            await _ticketCollection.InsertOneAsync(newticket);

        

        public async Task RegisterUserAsync(User user, string password)
        {
            user.PasswordHash = HashPassword(password);
            await _usersCollection.InsertOneAsync(user);
        }

        public async Task<User> AuthenticateAsync(string loginId, string password)
        {
            var user = await _usersCollection.Find(u => u.LoginId == loginId).FirstOrDefaultAsync();
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task UpdatePasswordAsync(string email, string newPassword)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, HashPassword(newPassword));
            await _usersCollection.UpdateOneAsync(filter, update);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;

        }
    }
}
