
using MovieTicketBooking.DTOs.Theatres;

namespace MovieTicketBooking.Services.Interfaces
{
    public interface ITheatreService
    {
        Task<List<TheatreResponse>> GetAllAsync();
        Task<TheatreResponse?> GetByIdAsync(int id);
        Task<TheatreResponse> CreateAsync(CreateTheatreRequest request);
        Task<TheatreResponse?> UpdateAsync(
            int id,
            UpdateTheatreRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
