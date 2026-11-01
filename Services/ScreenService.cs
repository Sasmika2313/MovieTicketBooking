
using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    public class ScreenService : IScreenService
    {
        private readonly IScreenRepository _repository;

        public ScreenService(IScreenRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ScreenResponse>> GetAllAsync(
            int? theatreId)
        {
            var screens = await _repository.GetAllAsync(theatreId);

            return screens.Select(MapToResponse).ToList();
        }

        public async Task<ScreenResponse?> GetByIdAsync(int id)
        {
            var screen = await _repository.GetByIdAsync(id);

            return screen == null ? null : MapToResponse(screen);
        }

        public async Task<List<ScreenResponse>> GetByTheatreIdAsync(
            int theatreId)
        {
            var screens = await _repository
                .GetByTheatreIdAsync(theatreId);

            return screens.Select(MapToResponse).ToList();
        }

        public Task<bool> TheatreExistsAsync(int theatreId)
        {
            return _repository.TheatreExistsAsync(theatreId);
        }

        public async Task<ScreenResponse?> CreateAsync(
            CreateScreenRequest request)
        {
            var screenName = request.ScreenName.Trim();

            if (await _repository.ScreenNameExistsAsync(
                request.TheatreId, screenName))
            {
                return null;
            }

            var screen = new Screen
            {
                TheatreId = request.TheatreId,
                ScreenName = screenName
            };

            var created = await _repository.AddAsync(screen);

            // Load the related theatre for the response
            var result = await _repository.GetByIdAsync(
                created.ScreenId);

            return result == null ? null : MapToResponse(result);
        }

        public async Task<ScreenResponse?> UpdateAsync(
            int id,
            UpdateScreenRequest request)
        {
            var screen = await _repository.GetByIdAsync(id);

            if (screen == null)
            {
                return null;
            }

            var screenName = request.ScreenName.Trim();

            if (await _repository.ScreenNameExistsAsync(
                request.TheatreId,
                screenName,
                id))
            {
                return null;
            }

            screen.TheatreId = request.TheatreId;
            screen.ScreenName = screenName;

            await _repository.UpdateAsync(screen);

            var updated = await _repository.GetByIdAsync(id);

            return updated == null ? null : MapToResponse(updated);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }

        private static ScreenResponse MapToResponse(Screen screen)
        {
            return new ScreenResponse
            {
                ScreenId = screen.ScreenId,
                TheatreId = screen.TheatreId,
                ScreenName = screen.ScreenName,
                TheatreName = screen.Theatre.Name
            };
        }
    }
}
