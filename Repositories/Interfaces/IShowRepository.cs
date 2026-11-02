using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IShowRepository
    {
        Task<List<Show>> GetAllAsync(
            DateTime? date,
            int? movieId,
            int? theatreId);

        Task<Show?> GetByIdAsync(int id);

        Task<List<Show>> GetByMovieIdAsync(int movieId);

        Task<List<Show>> GetByTheatreIdAsync(int theatreId);

        Task<List<ShowSeat>> GetShowSeatsAsync(
            int showId,
            string? status);

        Task<bool> MovieExistsAsync(int movieId);

        Task<bool> ScreenExistsAsync(int screenId);

        Task<bool> TheatreExistsAsync(int theatreId);

        Task<bool> ShowExistsAsync(int showId);

        Task<bool> HasOverlappingShowAsync(
            int screenId,
            DateTime startTime,
            DateTime endTime,
            int? excludeShowId = null);

        Task<Show> CreateWithSeatsAsync(Show show);

        Task<bool> UpdateWithSeatsAsync(Show show);

        Task<bool> DeleteAsync(int id);
    }
}