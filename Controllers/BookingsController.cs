using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MovieTicketBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking(
            [FromBody] CreateBookingRequest request)
        {
            if (!await _bookingService.ShowExistsAsync(request.ShowId))
            {
                return BadRequest("The specified show does not exist.");
            }

            try
            {
                var booking =
                    await _bookingService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetBookingById),
                    new { id = booking.BookingId },
                    booking);
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

        // GET: api/bookings
        // Optional filters: showId, status
        [HttpGet]
        public async Task<IActionResult> GetAllBookings(
            [FromQuery] int? showId,
            [FromQuery] string? status)
        {
            if (showId.HasValue && showId.Value <= 0)
            {
                return BadRequest("ShowId must be greater than zero.");
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();

                if (!status.Equals(
                        "Confirmed",
                        StringComparison.OrdinalIgnoreCase) &&
                    !status.Equals(
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(
                        "Status must be Confirmed or Cancelled.");
                }

                status = status.Equals(
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Confirmed"
                    : "Cancelled";
            }

            if (showId.HasValue &&
                !await _bookingService.ShowExistsAsync(showId.Value))
            {
                return NotFound("Show not found.");
            }

            var bookings =
                await _bookingService.GetAllAsync(showId, status);

            return Ok(bookings);
        }

        // GET: api/bookings/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Booking ID must be greater than zero.");
            }

            var booking = await _bookingService.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            return Ok(booking);
        }

        // GET: api/bookings/show/1
        [HttpGet("show/{showId:int}")]
        public async Task<IActionResult> GetBookingsByShow(int showId)
        {
            if (showId <= 0)
            {
                return BadRequest("Show ID must be greater than zero.");
            }

            if (!await _bookingService.ShowExistsAsync(showId))
            {
                return NotFound("Show not found.");
            }

            var bookings =
                await _bookingService.GetByShowIdAsync(showId);

            return Ok(bookings);
        }

        // PUT: api/bookings/1/cancel
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Booking ID must be greater than zero.");
            }

            try
            {
                var booking =
                    await _bookingService.CancelAsync(id);

                if (booking == null)
                {
                    return NotFound("Booking not found.");
                }

                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}