
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    public class ScreenRepository : IScreenRepository
    {
        private readonly ApplicationDbContext _context;

        public ScreenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Screen>> GetAllAsync(int? theatreId)
        {
            var query = _context.Screens
                .AsNoTracking()
                .Include(s => s.Theatre)
                .AsQueryable();

            if (theatreId.HasValue)
            {
                query = query.Where(
                    s => s.TheatreId == theatreId.Value);
            }

            
                return await query
                .OrderBy(s => s.ScreenId)
                .ToListAsync();
        }

        public async Task<Screen?> GetByIdAsync(int id)
        {
            return await _context.Screens
                .AsNoTracking()
                .Include(s => s.Theatre)
                .FirstOrDefaultAsync(s => s.ScreenId == id);
        }

        public async Task<List<Screen>> GetByTheatreIdAsync(
            int theatreId)
        {
            return await _context.Screens
                .AsNoTracking()
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId)
                .OrderBy(s => s.ScreenId)
                .ToListAsync();
        }

        public async Task<bool> TheatreExistsAsync(int theatreId)
        {
            return await _context.Theatres
                .AnyAsync(t => t.TheatreId == theatreId);
        }

        public async Task<bool> ScreenNameExistsAsync(
            int theatreId,
            string screenName,
            int? excludeScreenId = null)
        {
            var query = _context.Screens.Where(s =>
                s.TheatreId == theatreId &&
                s.ScreenName == screenName);

            if (excludeScreenId.HasValue)
            {
                query = query.Where(
                    s => s.ScreenId != excludeScreenId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Screen> AddAsync(Screen screen)
        {
            _context.Screens.Add(screen);
            await _context.SaveChangesAsync();

            return screen;
        }

        public async Task UpdateAsync(Screen screen)
        {
            _context.Screens.Update(screen);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var screen = await _context.Screens.FindAsync(id);

            if (screen == null)
            {
                return false;
            }

            _context.Screens.Remove(screen);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
