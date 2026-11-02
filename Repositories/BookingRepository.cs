using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Booking> BookingQuery()
        {
            return _context.Bookings
                .AsNoTracking()
                .Include(b => b.Show)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Show)
                    .ThenInclude(s => s.Screen)
                        .ThenInclude(sc => sc.Theatre)
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat);
        }

        public async Task<List<Booking>> GetAllAsync(
            int? showId,
            string? status)
        {
            var query = BookingQuery();

            if (showId.HasValue)
            {
                query = query.Where(
                    b => b.ShowId == showId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(
                    b => b.Status == status);
            }

            return await query
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await BookingQuery()
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<List<Booking>> GetByShowIdAsync(int showId)
        {
            return await BookingQuery()
                .Where(b => b.ShowId == showId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<bool> ShowExistsAsync(int showId)
        {
            return await _context.Shows
                .AnyAsync(s => s.ShowId == showId);
        }

        public async Task<Booking> CreateAsync(
            Booking booking,
            List<int> seatIds)
        {
            if (seatIds.Count == 0)
            {
                throw new InvalidOperationException(
                    "Select at least one seat.");
            }

            if (seatIds.Distinct().Count() != seatIds.Count)
            {
                throw new InvalidOperationException(
                    "Duplicate seat IDs are not allowed.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var show = await _context.Shows
                .FirstOrDefaultAsync(s => s.ShowId == booking.ShowId);

            if (show == null)
            {
                throw new KeyNotFoundException("Show not found.");
            }

            var requestedShowSeats = await _context.ShowSeats
                .Where(ss =>
                    ss.ShowId == booking.ShowId &&
                    seatIds.Contains(ss.SeatId))
                .ToListAsync();

            if (requestedShowSeats.Count != seatIds.Count)
            {
                throw new InvalidOperationException(
                    "One or more selected seats do not belong to this show.");
            }

            if (requestedShowSeats.Any(ss => ss.Status != "Available"))
            {
                throw new InvalidOperationException(
                    "One or more selected seats are no longer available.");
            }

            decimal seatPrice = show.TicketPrice;

            booking.TotalAmount = seatPrice * seatIds.Count;
            booking.BookingDate = DateTime.UtcNow;
            booking.Status = "Confirmed";

            // First save generates the BookingId.
            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            // Generate a unique booking reference using the database ID.
            booking.BookingReference = $"MOV{booking.BookingId}";

            var bookingSeats = requestedShowSeats.Select(ss =>
                new BookingSeat
                {
                    BookingId = booking.BookingId,
                    SeatId = ss.SeatId,
                    Price = seatPrice
                }).ToList();

            _context.BookingSeats.AddRange(bookingSeats);

            // Mark the corresponding seats as booked.
            foreach (var showSeat in requestedShowSeats)
            {
                showSeat.Status = "Booked";
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (await GetByIdAsync(booking.BookingId))!;
        }

        public async Task<Booking?> CancelAsync(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var booking = await _context.Bookings
                .Include(b => b.BookingSeats)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return null;
            }

            if (booking.Status != "Confirmed")
            {
                throw new InvalidOperationException(
                    "Only confirmed bookings can be cancelled.");
            }

            var seatIds = booking.BookingSeats
                .Select(bs => bs.SeatId)
                .ToList();

            var showSeats = await _context.ShowSeats
                .Where(ss =>
                    ss.ShowId == booking.ShowId &&
                    seatIds.Contains(ss.SeatId))
                .ToListAsync();

            booking.Status = "Cancelled";

            foreach (var showSeat in showSeats)
            {
                showSeat.Status = "Available";
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetByIdAsync(id);
        }
    }
}