using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;

        public ShowService(IShowRepository showRepository)
        {
            _showRepository = showRepository;
        }

        public async Task<List<ShowResponse>> GetAllAsync(
            DateTime? date,
            int? movieId,
            int? theatreId)
        {
            var shows = await _showRepository.GetAllAsync(
                date, movieId, theatreId);

            return shows.Select(MapToResponse).ToList();
        }

        public async Task<ShowResponse?> GetByIdAsync(int id)
        {
            var show = await _showRepository.GetByIdAsync(id);

            return show == null ? null : MapToResponse(show);
        }

        public async Task<List<ShowResponse>> GetByMovieIdAsync(int movieId)
        {
            var shows =
                await _showRepository.GetByMovieIdAsync(movieId);

            return shows.Select(MapToResponse).ToList();
        }

        public async Task<List<ShowResponse>> GetByTheatreIdAsync(
            int theatreId)
        {
            var shows =
                await _showRepository.GetByTheatreIdAsync(theatreId);

            return shows.Select(MapToResponse).ToList();
        }

        public async Task<List<ShowSeatResponse>?> GetShowSeatsAsync(
            int showId,
            string? status)
        {
            bool exists = await _showRepository.ShowExistsAsync(showId);

            if (!exists)
            {
                return null;
            }

            var showSeats =
                await _showRepository.GetShowSeatsAsync(showId, status);

            return showSeats.Select(ss => new ShowSeatResponse
            {
                ShowSeatId = ss.ShowSeatId,
                ShowId = ss.ShowId,
                SeatId = ss.SeatId,
                SeatNumber = ss.Seat.SeatNumber,
                SeatType = ss.Seat.SeatType,
                Status = ss.Status,
                RowName = ss.Seat.RowName
            }).ToList();
        }

        public async Task<bool> MovieExistsAsync(int movieId)
        {
            return await _showRepository.MovieExistsAsync(movieId);
        }

        public async Task<bool> ScreenExistsAsync(int screenId)
        {
            return await _showRepository.ScreenExistsAsync(screenId);
        }

        public async Task<bool> TheatreExistsAsync(int theatreId)
        {
            return await _showRepository.TheatreExistsAsync(theatreId);
        }

        public async Task<ShowResponse?> CreateAsync(
            CreateShowRequest request)
        {
            if (request.EndTime <= request.StartTime)
            {
                throw new ArgumentException(
                    "EndTime must be later than StartTime.");
            }

            bool overlap =
                await _showRepository.HasOverlappingShowAsync(
                    request.ScreenId,
                    request.StartTime,
                    request.EndTime);

            if (overlap)
            {
                throw new InvalidOperationException(
                    "The selected screen already has a show scheduled during this time.");
            }

            var show = new Show
            {
                MovieId = request.MovieId,
                ScreenId = request.ScreenId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                TicketPrice = request.TicketPrice
            };

            var createdShow =
                await _showRepository.CreateWithSeatsAsync(show);

            var savedShow =
                await _showRepository.GetByIdAsync(createdShow.ShowId);

            return savedShow == null ? null : MapToResponse(savedShow);
        }

        public async Task<ShowResponse?> UpdateAsync(
            int id,
            UpdateShowRequest request)
        {
            if (request.EndTime <= request.StartTime)
            {
                throw new ArgumentException(
                    "EndTime must be later than StartTime.");
            }

            var existingShow =
                await _showRepository.GetByIdAsync(id);

            if (existingShow == null)
            {
                return null;
            }

            bool overlap =
                await _showRepository.HasOverlappingShowAsync(
                    request.ScreenId,
                    request.StartTime,
                    request.EndTime,
                    id);

            if (overlap)
            {
                throw new InvalidOperationException(
                    "The selected screen already has a show scheduled during this time.");
            }

            var updatedShow = new Show
            {
                ShowId = id,
                MovieId = request.MovieId,
                ScreenId = request.ScreenId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                TicketPrice = request.TicketPrice
            };

            bool updated =
                await _showRepository.UpdateWithSeatsAsync(updatedShow);

            if (!updated)
            {
                throw new InvalidOperationException(
                    "This show cannot be updated because it has booked seats.");
            }

            var savedShow =
                await _showRepository.GetByIdAsync(id);

            return savedShow == null ? null : MapToResponse(savedShow);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _showRepository.DeleteAsync(id);
        }

        private static ShowResponse MapToResponse(Show show)
        {
            return new ShowResponse
            {
                ShowId = show.ShowId,
                MovieId = show.MovieId,
                MovieTitle = show.Movie.Title,
                ScreenId = show.ScreenId,
                ScreenName = show.Screen.ScreenName,
                TheatreId = show.Screen.TheatreId,
                TheatreName = show.Screen.Theatre.Name,
                StartTime = show.StartTime,
                EndTime = show.EndTime,
                TicketPrice = show.TicketPrice,
                TotalSeats = show.ShowSeats.Count
            };
        }
    }
}