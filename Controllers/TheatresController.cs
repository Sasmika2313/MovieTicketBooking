
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Services.Interfaces;
using MovieTicketBooking.Constants;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheatresController : ControllerBase
    {
        private readonly ITheatreService _service;

        public TheatresController(ITheatreService service)
        {
            _service = service;
        }

        // POST: api/theatres
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateTheatreRequest request)
        {
            var theatre = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = theatre.TheatreId },
                theatre);
        }

        // GET: api/theatres
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var theatres = await _service.GetAllAsync();

            return Ok(theatres);
        }

        // GET: api/theatres/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var theatre = await _service.GetByIdAsync(id);

            if (theatre == null)
            {
                return NotFound(new
                {
                    message = "Theatre not found."
                });
            }

            return Ok(theatre);
        }

        // PUT: api/theatres/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateTheatreRequest request)
        {
            var theatre = await _service.UpdateAsync(id, request);

            if (theatre == null)
            {
                return NotFound(new
                {
                    message = "Theatre not found."
                });
            }

            return Ok(theatre);
        }

        // DELETE: api/theatres/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Theatre not found."
                });
            }

            return NoContent();
        }
    }
}
