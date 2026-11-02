using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Seats
{
    public interface ISeatRepository
    {
        Task<List<Seat>> GetAllAsync(int? screenId);

        Task<Seat?> GetByIdAsync(int id);

        Task<List<Seat>> GetByScreenIdAsync(int screenId);

        Task<bool> ScreenExistsAsync(int screenId);

        Task<bool> CoordinateExistsAsync(
            int screenId,
            string rowName,
            int seatNumber,
            int? excludeSeatId = null);

        Task<Seat> CreateAsync(Seat seat);

        Task<List<Seat>> CreateBulkAsync(List<Seat> seats);

        Task UpdateAsync(Seat seat);

        Task<bool> HasDependenciesAsync(int seatId);

        Task<bool> DeleteAsync(int id);
    }
}