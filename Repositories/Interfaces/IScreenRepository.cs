
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IScreenRepository
    {
        Task<List<Screen>> GetAllAsync(int? theatreId);

        Task<Screen?> GetByIdAsync(int id);

        Task<List<Screen>> GetByTheatreIdAsync(int theatreId);

        Task<bool> TheatreExistsAsync(int theatreId);

        Task<bool> ScreenNameExistsAsync(
            int theatreId,
            string screenName,
            int? excludeScreenId = null);

        Task<Screen> AddAsync(Screen screen);

        Task UpdateAsync(Screen screen);

        Task<bool> DeleteAsync(int id);
    }
}
