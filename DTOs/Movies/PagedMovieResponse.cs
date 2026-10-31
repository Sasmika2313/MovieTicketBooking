
namespace MovieTicketBooking.DTOs.Movies;

public class PagedMoviesResponse
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public List<MovieResponse> Movies { get; set; } = new();
}
