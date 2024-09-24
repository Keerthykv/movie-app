
using movie_app.Models;
using movie_app.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.AspNetCore.Cors;

namespace movie_app.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
[EnableCors("AllowAllOrigins")]
public class MovieBookingController : ControllerBase
{
    private readonly MongoDBServices _movieService;
    private readonly MongoDBServices _ticketService;
    private readonly MongoDBServices _userService;

    public MovieBookingController(MongoDBServices MovieService, MongoDBServices TicketServices, MongoDBServices UserServices)
    {
        _movieService = MovieService;
        _ticketService = TicketServices;
        _userService = UserServices;
    }


    [HttpPost("register")]
    [EnableCors("AllowAllOrigins")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            LoginId = model.LoginId,
            ContactNumber = model.ContactNumber
        };

        await _userService.RegisterUserAsync(user, model.Password);
        return Ok("User registered successfully.");
    }

    [HttpPost("Login")]
    [EnableCors("AllowAllOrigins")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userService.AuthenticateAsync(model.LoginId, model.Password);
        if (user == null)
        {
            return Unauthorized("Invalid login ID or password.");
        }

        return Ok(new { user });
    }

    [HttpPost("{username}/forgot")]
    [EnableCors("AllowAllOrigins")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        var user = await _userService.GetUserByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        await _userService.UpdatePasswordAsync(model.Email, model.NewPassword);
        return Ok("Password updated successfully.");
    }

    [HttpGet("all")]
    [EnableCors("AllowAllOrigins")]
    public async Task<List<MovieBooking>> all() =>
        await _movieService.GetAsync();

    [HttpGet("movies/search/{*moviename}")]
    [EnableCors("AllowAllOrigins")]
    public async Task<ActionResult<List<MovieBooking>>> movies(string moviename)
    {
        // var filter = Builders<MovieBooking>.Filter.Eq("name", name);
        //var result = await .Find(filter).FirstOrDefaultAsync();
        List<MovieBooking> movie = new List<MovieBooking>();
        movie=await _movieService.GetAsync(moviename);

        if (movie is null)
        {
            return NotFound();
        }

        return movie;
    }

    [HttpPost("{moviename}/add")]
    [EnableCors("AllowAllOrigins")]
    public async Task<IActionResult> Tickets(TicketBooking newTicket)
    {
        bool isAvailable = await _movieService.IsMovieInTheaterAsync(newTicket.moviename, newTicket.theatre);

        if (!isAvailable)
        {
            return BadRequest("The movie is not available in the specified theater.");
        }
        await _ticketService.CreateAsync(newTicket);
        bool updateSuccessful = await _movieService.UpdateAvailableTicketsAsync(newTicket.moviename, newTicket.theatre, newTicket.no_tickets,newTicket.seat_no);

        if (!updateSuccessful)
        {
            return StatusCode(500, "Failed to update the number of available tickets.");
        }
        return CreatedAtAction(nameof(movies), new { moviename = newTicket.moviename }, newTicket);
    }

    [HttpPut("{moviename}/update")]
    [EnableCors("AllowAllOrigins")]
    public async Task<IActionResult> UpdateTickets(TicketBooking newTicket)
    {
        // Find the movie to ensure it exists and has enough seats
        List<MovieBooking> movie = new List<MovieBooking>();
        movie = await _movieService.GetAsync(newTicket.moviename);

        if (movie == null)
        {
            return NotFound("Movie not found");
        }

       // if (movie.no_tickets < newTicket.no_tickets)
        //{
       //     return BadRequest("Not enough seats available");
        //}


        // Update the number of available seats
        await _movieService.UpdateMovieAsync(newTicket.moviename, newTicket.theatre, newTicket.no_tickets, newTicket.seat_no);
        await _ticketService.CreateAsync(newTicket);
        return CreatedAtAction(nameof(movies), new { moviename = newTicket.moviename }, newTicket);
    }

    [HttpDelete("{movieName}/delete/{id}")]
    [EnableCors("AllowAllOrigins")]
    public async Task<IActionResult> DeleteMovie(string movieName)
    {
        // Find the movie to ensure it exists
        var movie = await _movieService.GetAsync(movieName);
        if (movie == null)
        {
            return NotFound("Movie not found");
        }

        // Delete the movie and related tickets
        await _movieService.DeleteMovieAsync(movieName);

        return NoContent();
    }

}

public class RegisterModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string LoginId { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string ContactNumber { get; set; }
}

public class LoginModel
{
    public string LoginId { get; set; }
    public string Password { get; set; }
}

public class ForgotPasswordModel
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}




