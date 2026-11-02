
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Services.Interfaces;
using MovieTicketBooking.Constants;

namespace MovieTicketBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowsController(IShowService showService)
        {
            _showService = showService;
        }

        // POST: api/shows
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateShow(
            [FromBody] CreateShowRequest request)
        {
            if (!await _showService.MovieExistsAsync(request.MovieId))
            {
                return BadRequest("The specified movie does not exist.");
            }

            if (!await _showService.ScreenExistsAsync(request.ScreenId))
            {
                return BadRequest("The specified screen does not exist.");
            }

            try
            {
                var show = await _showService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetShowById),
                    new { id = show!.ShowId },
                    show);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // GET: api/shows
        [HttpGet]
        public async Task<IActionResult> GetAllShows(
            [FromQuery] DateTime? date,
            [FromQuery] int? movieId,
            [FromQuery] int? theatreId)
        {
            if (movieId.HasValue && movieId.Value <= 0)
            {
                return BadRequest("MovieId must be greater than zero.");
            }

            if (theatreId.HasValue && theatreId.Value <= 0)
            {
                return BadRequest("TheatreId must be greater than zero.");
            }

            if (movieId.HasValue &&
                !await _showService.MovieExistsAsync(movieId.Value))
            {
                return NotFound("Movie not found.");
            }

            if (theatreId.HasValue &&
                !await _showService.TheatreExistsAsync(theatreId.Value))
            {
                return NotFound("Theatre not found.");
            }

            var shows = await _showService.GetAllAsync(
                date, movieId, theatreId);

            return Ok(shows);
        }

        // GET: api/shows/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetShowById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            var show = await _showService.GetByIdAsync(id);

            if (show == null)
            {
                return NotFound("Show not found.");
            }

            return Ok(show);
        }

        // GET: api/shows/movie/1
        [HttpGet("movie/{movieId:int}")]
        public async Task<IActionResult> GetShowsByMovie(int movieId)
        {
            if (movieId <= 0)
            {
                return BadRequest("Movie ID must be greater than zero.");
            }

            if (!await _showService.MovieExistsAsync(movieId))
            {
                return NotFound("Movie not found.");
            }

            var shows = await _showService.GetByMovieIdAsync(movieId);

            return Ok(shows);
        }

        // GET: api/shows/theatre/1
        [HttpGet("theatre/{theatreId:int}")]
        public async Task<IActionResult> GetShowsByTheatre(int theatreId)
        {
            if (theatreId <= 0)
            {
                return BadRequest("Theatre ID must be greater than zero.");
            }

            if (!await _showService.TheatreExistsAsync(theatreId))
            {
                return NotFound("Theatre not found.");
            }

            var shows =
                await _showService.GetByTheatreIdAsync(theatreId);

            return Ok(shows);
        }

        // GET: api/shows/search?date=2026-11-01
        [HttpGet("search")]
        public async Task<IActionResult> SearchShows(
            [FromQuery] DateTime date)
        {
            var shows = await _showService.GetAllAsync(
                date.Date, null, null);

            return Ok(shows);
        }

        // GET: api/shows/1/seats
        [HttpGet("{showId:int}/seats")]
        public async Task<IActionResult> GetShowSeats(int showId)
        {
            if (showId <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            var seats = await _showService.GetShowSeatsAsync(
                showId, null);

            if (seats == null)
            {
                return NotFound("Show not found.");
            }

            return Ok(seats);
        }

        // GET: api/shows/1/seats/available
        [HttpGet("{showId:int}/seats/available")]
        public async Task<IActionResult> GetAvailableSeats(int showId)
        {
            if (showId <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            var seats = await _showService.GetShowSeatsAsync(
                showId, "Available");

            if (seats == null)
            {
                return NotFound("Show not found.");
            }

            return Ok(seats);
        }

        // GET: api/shows/1/seats/booked
        [HttpGet("{showId:int}/seats/booked")]
        public async Task<IActionResult> GetBookedSeats(int showId)
        {
            if (showId <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            var seats = await _showService.GetShowSeatsAsync(
                showId, "Booked");

            if (seats == null)
            {
                return NotFound("Show not found.");
            }

            return Ok(seats);
        }

        // PUT: api/shows/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateShow(
            int id,
            [FromBody] UpdateShowRequest request)
        {
            if (id <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            if (!await _showService.MovieExistsAsync(request.MovieId))
            {
                return BadRequest("The specified movie does not exist.");
            }

            if (!await _showService.ScreenExistsAsync(request.ScreenId))
            {
                return BadRequest("The specified screen does not exist.");
            }

            try
            {
                var show = await _showService.UpdateAsync(id, request);

                if (show == null)
                {
                    return NotFound("Show not found.");
                }

                return Ok(show);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // DELETE: api/shows/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteShow(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            bool deleted = await _showService.DeleteAsync(id);

            if (!deleted)
            {
                return Conflict(
                    "Show not found or it cannot be deleted because it has booked seats.");
            }

            return NoContent();
        }
    }
}
