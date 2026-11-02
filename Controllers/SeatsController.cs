
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Services.Seats;
using MovieTicketBooking.Constants;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/seats")]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _service;

        public SeatsController(ISeatService service)
        {
            _service = service;
        }

        // GET: api/seats
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? screenId)
        {
            var seats = await _service.GetAllAsync(screenId);
            return Ok(seats);
        }

        // GET: api/seats/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var seat = await _service.GetByIdAsync(id);

            if (seat == null)
            {
                return NotFound("Seat not found.");
            }

            return Ok(seat);
        }

        // GET: api/seats/screen/1
        [HttpGet("screen/{screenId:int}")]
        public async Task<IActionResult> GetByScreen(int screenId)
        {
            try
            {
                var seats = await _service.GetByScreenIdAsync(screenId);
                return Ok(seats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/seats
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateSeatRequest request)
        {
            try
            {
                var seat = await _service.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = seat.SeatId },
                    seat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // POST: api/seats/bulk
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk(
            BulkCreateSeatsRequest request)
        {
            try
            {
                var seats = await _service.CreateBulkAsync(request);

                return StatusCode(
                    StatusCodes.Status201Created,
                    seats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/seats/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateSeatRequest request)
        {
            try
            {
                var seat = await _service.UpdateAsync(id, request);

                if (seat == null)
                {
                    return NotFound("Seat not found.");
                }

                return Ok(seat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // DELETE: api/seats/1
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return Conflict(
                    "Seat was not found or cannot be deleted because it is referenced by a show or booking.");
            }

            return NoContent();
        }
    }
}
