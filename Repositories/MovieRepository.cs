
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _context;

    public MovieRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Movie>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? title)
    {
        var query = _context.Movies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(m => m.Title.Contains(title));
        }

        return await query
            .OrderBy(m => m.MovieId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? title)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(m => m.Title.Contains(title));
        }

        return await query.CountAsync();
    }

    public async Task<Movie?> GetByIdAsync(int id)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(m => m.MovieId == id);
    }

    public async Task<List<Movie>> SearchAsync(string title)
    {
        return await _context.Movies
            .AsNoTracking()
            .Where(m => m.Title.Contains(title))
            .OrderBy(m => m.Title)
            .ToListAsync();
    }

    public async Task<Movie> AddAsync(Movie movie)
    {
        await _context.Movies.AddAsync(movie);

        await _context.SaveChangesAsync();

        return movie;
    }

    public async Task UpdateAsync(Movie movie)
    {
        _context.Movies.Update(movie);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return false;
        }

        _context.Movies.Remove(movie);

        await _context.SaveChangesAsync();

        return true;
    }
}
