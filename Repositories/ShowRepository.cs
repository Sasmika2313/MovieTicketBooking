using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    public class ShowRepository : IShowRepository
    {
        private readonly ApplicationDbContext _context;

        public ShowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Show> ShowQuery()
        {
            return _context.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                    .ThenInclude(sc => sc.Theatre)
                .Include(s => s.ShowSeats);
        }

        public async Task<List<Show>> GetAllAsync(
            DateTime? date,
            int? movieId,
            int? theatreId)
        {
            var query = ShowQuery();

            if (date.HasValue)
            {
                DateTime dayStart = date.Value.Date;
                DateTime dayEnd = dayStart.AddDays(1);

                query = query.Where(s =>
                    s.StartTime >= dayStart &&
                    s.StartTime < dayEnd);
            }

            if (movieId.HasValue)
            {
                query = query.Where(s => s.MovieId == movieId.Value);
            }

            if (theatreId.HasValue)
            {
                query = query.Where(
                    s => s.Screen.TheatreId == theatreId.Value);
            }

            return await query
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Show?> GetByIdAsync(int id)
        {
            return await ShowQuery()
                .FirstOrDefaultAsync(s => s.ShowId == id);
        }

        public async Task<List<Show>> GetByMovieIdAsync(int movieId)
        {
            return await ShowQuery()
                .Where(s => s.MovieId == movieId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Show>> GetByTheatreIdAsync(int theatreId)
        {
            return await ShowQuery()
                .Where(s => s.Screen.TheatreId == theatreId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<ShowSeat>> GetShowSeatsAsync(
            int showId,
            string? status)
        {
            var query = _context.ShowSeats
                .AsNoTracking()
                .Include(ss => ss.Seat)
                .Where(ss => ss.ShowId == showId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(ss => ss.Status == status);
            }

            return await query
                .OrderBy(ss => ss.Seat.SeatNumber)
                .ToListAsync();
        }

        public async Task<bool> MovieExistsAsync(int movieId)
        {
            return await _context.Movies
                .AnyAsync(m => m.MovieId == movieId);
        }

        public async Task<bool> ScreenExistsAsync(int screenId)
        {
            return await _context.Screens
                .AnyAsync(s => s.ScreenId == screenId);
        }

        public async Task<bool> TheatreExistsAsync(int theatreId)
        {
            return await _context.Theatres
                .AnyAsync(t => t.TheatreId == theatreId);
        }

        public async Task<bool> ShowExistsAsync(int showId)
        {
            return await _context.Shows
                .AnyAsync(s => s.ShowId == showId);
        }

        public async Task<bool> HasOverlappingShowAsync(
            int screenId,
            DateTime startTime,
            DateTime endTime,
            int? excludeShowId = null)
        {
            var query = _context.Shows.Where(s =>
                s.ScreenId == screenId &&
                startTime < s.EndTime &&
                endTime > s.StartTime);

            if (excludeShowId.HasValue)
            {
                query = query.Where(
                    s => s.ShowId != excludeShowId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Show> CreateWithSeatsAsync(Show show)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            _context.Shows.Add(show);

            await _context.SaveChangesAsync();

            var seats = await _context.Seats
                .Where(s => s.ScreenId == show.ScreenId)
                .ToListAsync();

            if (seats.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot create a show because this screen has no seats.");
            }

            var showSeats = seats.Select(seat => new ShowSeat
            {
                ShowId = show.ShowId,
                SeatId = seat.SeatId,
                Status = "Available"
            }).ToList();

            _context.ShowSeats.AddRange(showSeats);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return show;
        }

        public async Task<bool> UpdateWithSeatsAsync(Show show)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var existingShow = await _context.Shows
                .FirstOrDefaultAsync(s => s.ShowId == show.ShowId);

            if (existingShow == null)
            {
                return false;
            }

            bool hasBookedSeats = await _context.ShowSeats
                .AnyAsync(ss =>
                    ss.ShowId == show.ShowId &&
                    ss.Status == "Booked");

            if (hasBookedSeats)
            {
                return false;
            }

            var oldShowSeats = await _context.ShowSeats
                .Where(ss => ss.ShowId == show.ShowId)
                .ToListAsync();

            _context.ShowSeats.RemoveRange(oldShowSeats);

            existingShow.MovieId = show.MovieId;
            existingShow.ScreenId = show.ScreenId;
            existingShow.StartTime = show.StartTime;
            existingShow.EndTime = show.EndTime;
            existingShow.TicketPrice = show.TicketPrice;

            await _context.SaveChangesAsync();

            var seats = await _context.Seats
                .Where(s => s.ScreenId == show.ScreenId)
                .ToListAsync();

            var newShowSeats = seats.Select(seat => new ShowSeat
            {
                ShowId = show.ShowId,
                SeatId = seat.SeatId,
                Status = "Available"
            }).ToList();

            _context.ShowSeats.AddRange(newShowSeats);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var show = await _context.Shows
                .FirstOrDefaultAsync(s => s.ShowId == id);

            if (show == null)
            {
                return false;
            }

            bool hasBookedSeats = await _context.ShowSeats
                .AnyAsync(ss =>
                    ss.ShowId == id &&
                    ss.Status == "Booked");

            if (hasBookedSeats)
            {
                return false;
            }

            var showSeats = await _context.ShowSeats
                .Where(ss => ss.ShowId == id)
                .ToListAsync();

            _context.ShowSeats.RemoveRange(showSeats);
            _context.Shows.Remove(show);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}