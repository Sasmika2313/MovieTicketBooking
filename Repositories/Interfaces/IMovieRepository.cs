
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? title);

    Task<int> GetCountAsync(string? title);

    Task<Movie?> GetByIdAsync(int id);

    Task<List<Movie>> SearchAsync(string title);

    Task<Movie> AddAsync(Movie movie);

    Task UpdateAsync(Movie movie);

    Task<bool> DeleteAsync(int id);
}
