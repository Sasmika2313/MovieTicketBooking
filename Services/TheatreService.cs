
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    public class TheatreService : ITheatreService
    {
        private readonly ITheatreRepository _repository;

        public TheatreService(ITheatreRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TheatreResponse>> GetAllAsync()
        {
            var theatres = await _repository.GetAllAsync();

            return theatres.Select(MapToResponse).ToList();
        }

        public async Task<TheatreResponse?> GetByIdAsync(int id)
        {
            var theatre = await _repository.GetByIdAsync(id);

            return theatre == null ? null : MapToResponse(theatre);
        }

        public async Task<TheatreResponse> CreateAsync(
            CreateTheatreRequest request)
        {
            var theatre = new Theatre
            {
                Name = request.Name.Trim(),
                Location = request.Location.Trim()
            };

            var created = await _repository.AddAsync(theatre);

            return MapToResponse(created);
        }

        public async Task<TheatreResponse?> UpdateAsync(
            int id,
            UpdateTheatreRequest request)
        {
            var theatre = await _repository.GetByIdAsync(id);

            if (theatre == null)
            {
                return null;
            }

            theatre.Name = request.Name.Trim();
            theatre.Location = request.Location.Trim();

            await _repository.UpdateAsync(theatre);

            return MapToResponse(theatre);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }

        private static TheatreResponse MapToResponse(Theatre theatre)
        {
            return new TheatreResponse
            {
                TheatreId = theatre.TheatreId,
                Name = theatre.Name,
                Location = theatre.Location
            };
        }
    }
}
