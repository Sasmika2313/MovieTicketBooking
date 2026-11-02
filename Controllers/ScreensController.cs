
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Services.Interfaces;
using MovieTicketBooking.Constants;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreensController : ControllerBase
    {
        private readonly IScreenService _service;

        public ScreensController(IScreenService service)
        {
            _service = service;
        }

        // POST: api/screens
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateScreenRequest request)
        {
            var theatreExists = await _service
                .TheatreExistsAsync(request.TheatreId);

            if (!theatreExists)
            {
                return BadRequest(new
                {
                    message = "The specified theatre does not exist."
                });
            }

            var screen = await _service.CreateAsync(request);

            if (screen == null)
            {
                return Conflict(new
                {
                    message = "A screen with this name already exists in this theatre."
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = screen.ScreenId },
                screen);
        }

        // GET: api/screens
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? theatreId)
        {
            if (theatreId.HasValue && theatreId.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "TheatreId must be greater than zero."
                });
            }

            if (theatreId.HasValue &&
                !await _service.TheatreExistsAsync(theatreId.Value))
            {
                return NotFound(new
                {
                    message = "Theatre not found."
                });
            }

            var screens = await _service.GetAllAsync(theatreId);

            return Ok(screens);
        }

        // GET: api/screens/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var screen = await _service.GetByIdAsync(id);

            if (screen == null)
            {
                return NotFound(new
                {
                    message = "Screen not found."
                });
            }

            return Ok(screen);
        }

        // GET: api/screens/theatre/1
        [HttpGet("theatre/{theatreId:int}")]
        public async Task<IActionResult> GetByTheatre(int theatreId)
        {
            if (theatreId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid TheatreId."
                });
            }

            if (!await _service.TheatreExistsAsync(theatreId))
            {
                return NotFound(new
                {
                    message = "Theatre not found."
                });
            }

            var screens = await _service
                .GetByTheatreIdAsync(theatreId);

            return Ok(screens);
        }

        // PUT: api/screens/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateScreenRequest request)
        {
            var theatreExists = await _service
                .TheatreExistsAsync(request.TheatreId);

            if (!theatreExists)
            {
                return BadRequest(new
                {
                    message = "The specified theatre does not exist."
                });
            }

            var screen = await _service.UpdateAsync(id, request);

            if (screen == null)
            {
                var existing = await _service.GetByIdAsync(id);

                if (existing == null)
                {
                    return NotFound(new
                    {
                        message = "Screen not found."
                    });
                }

                return Conflict(new
                {
                    message = "A screen with this name already exists in this theatre."
                });
            }

            return Ok(screen);
        }

        // DELETE: api/screens/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Screen not found."
                });
            }

            return NoContent();
        }
    }
}
