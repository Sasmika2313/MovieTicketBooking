using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Seats
{
    public class SeatRepository : ISeatRepository
    {
        private readonly ApplicationDbContext _context;

        public SeatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Seat>> GetAllAsync(int? screenId)
        {
            var query = _context.Seats
                .AsNoTracking()
                .Include(s => s.Screen)
                .AsQueryable();

            if (screenId.HasValue)
            {
                query = query.Where(s => s.ScreenId == screenId.Value);
            }

            return await query
                .OrderBy(s => s.RowName)
                .ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<Seat?> GetByIdAsync(int id)
        {
            return await _context.Seats
                .AsNoTracking()
                .Include(s => s.Screen)
                .FirstOrDefaultAsync(s => s.SeatId == id);
        }

        public async Task<List<Seat>> GetByScreenIdAsync(int screenId)
        {
            return await _context.Seats
                .AsNoTracking()
                .Include(s => s.Screen)
                .Where(s => s.ScreenId == screenId)
                .OrderBy(s => s.RowName)
                .ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<bool> ScreenExistsAsync(int screenId)
        {
            return await _context.Screens
                .AnyAsync(s => s.ScreenId == screenId);
        }

        public async Task<bool> CoordinateExistsAsync(
            int screenId,
            string rowName,
            int seatNumber,
            int? excludeSeatId = null)
        {
            var query = _context.Seats.Where(s =>
                s.ScreenId == screenId &&
                s.RowName == rowName &&
                s.SeatNumber == seatNumber);

            if (excludeSeatId.HasValue)
            {
                query = query.Where(s => s.SeatId != excludeSeatId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Seat> CreateAsync(Seat seat)
        {
            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            return seat;
        }

        public async Task<List<Seat>> CreateBulkAsync(List<Seat> seats)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            _context.Seats.AddRange(seats);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return seats;
        }

        public async Task UpdateAsync(Seat seat)
        {
            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasDependenciesAsync(int seatId)
        {
            return await _context.ShowSeats
                       .AnyAsync(ss => ss.SeatId == seatId)
                   ||
                   await _context.BookingSeats
                       .AnyAsync(bs => bs.SeatId == seatId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var seat = await _context.Seats.FindAsync(id);

            if (seat == null)
            {
                return false;
            }

            if (await HasDependenciesAsync(id))
            {
                return false;
            }

            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}