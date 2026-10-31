
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _service;

    public MoviesController(IMovieService service)
    {
        _service = service;
    }

    // POST: api/movies
    [HttpPost]
    public async Task<IActionResult> CreateMovie(
        [FromBody] CreateMovieRequest request)
    {
        var movie = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetMovieById),
            new { id = movie.MovieId },
            movie);
    }

    // GET: api/movies
    [HttpGet]
    public async Task<IActionResult> GetAllMovies(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? title = null)
    {
        var movies = await _service.GetAllAsync(
            pageNumber, pageSize, title);

        return Ok(movies);
    }

    // GET: api/movies/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMovieById(int id)
    {
        var movie = await _service.GetByIdAsync(id);

        if (movie == null)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return Ok(movie);
    }

    // PUT: api/movies/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMovie(
        int id,
        [FromBody] UpdateMovieRequest request)
    {
        var movie = await _service.UpdateAsync(id, request);

        if (movie == null)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return Ok(movie);
    }

    // DELETE: api/movies/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return NoContent();
    }

    // GET: api/movies/search?title=leo
    [HttpGet("search")]
    public async Task<IActionResult> SearchMovies(
        [FromQuery] string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new
            {
                message = "Search title is required."
            });
        }

        var movies = await _service.SearchAsync(title);

        return Ok(movies);
    }
}
