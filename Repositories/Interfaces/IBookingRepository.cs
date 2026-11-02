using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync(int? showId, string? status);

        Task<Booking?> GetByIdAsync(int id);

        Task<List<Booking>> GetByShowIdAsync(int showId);

        Task<bool> ShowExistsAsync(int showId);

        Task<Booking> CreateAsync(
            Booking booking,
            List<int> seatIds);

        Task<Booking?> CancelAsync(int id);
    }
}