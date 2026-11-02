using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<List<BookingResponse>> GetAllAsync(
            int? showId,
            string? status)
        {
            var bookings = await _bookingRepository.GetAllAsync(
                showId, status);

            return bookings.Select(MapToResponse).ToList();
        }

        public async Task<BookingResponse?> GetByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            return booking == null ? null : MapToResponse(booking);
        }

        public async Task<List<BookingResponse>> GetByShowIdAsync(
            int showId)
        {
            var bookings =
                await _bookingRepository.GetByShowIdAsync(showId);

            return bookings.Select(MapToResponse).ToList();
        }

        public async Task<bool> ShowExistsAsync(int showId)
        {
            return await _bookingRepository.ShowExistsAsync(showId);
        }

        public async Task<BookingResponse> CreateAsync(
            CreateBookingRequest request)
        {
            var booking = new Booking
            {
                ShowId = request.ShowId,
                CustomerName = request.CustomerName.Trim(),
                CustomerEmail = request.CustomerEmail.Trim(),
                CustomerPhone = request.CustomerPhone.Trim()
            };

            var createdBooking = await _bookingRepository.CreateAsync(
                booking,
                request.SeatIds);

            return MapToResponse(createdBooking);
        }

        public async Task<BookingResponse?> CancelAsync(int id)
        {
            var booking = await _bookingRepository.CancelAsync(id);

            return booking == null ? null : MapToResponse(booking);
        }

        private static BookingResponse MapToResponse(Booking booking)
        {
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,
                ShowId = booking.ShowId,
                MovieTitle = booking.Show.Movie.Title,
                TheatreName = booking.Show.Screen.Theatre.Name,
                ScreenName = booking.Show.Screen.ScreenName,
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                CustomerPhone = booking.CustomerPhone,
                BookingDate = booking.BookingDate,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                Seats = booking.BookingSeats
                    .Select(bs => $"{bs.Seat.RowName}{bs.Seat.SeatNumber}")
                    .ToList()
            };
        }
    }
}