using MovieTicketBooking.DTOs.Seats;

namespace MovieTicketBooking.Services.Seats
{
    public interface ISeatService
    {
        Task<List<SeatResponse>> GetAllAsync(int? screenId);

        Task<SeatResponse?> GetByIdAsync(int id);

        Task<List<SeatResponse>> GetByScreenIdAsync(int screenId);

        Task<SeatResponse> CreateAsync(CreateSeatRequest request);

        Task<List<SeatResponse>> CreateBulkAsync(
            BulkCreateSeatsRequest request);

        Task<SeatResponse?> UpdateAsync(
            int id,
            UpdateSeatRequest request);

        Task<bool> DeleteAsync(int id);
    }
}