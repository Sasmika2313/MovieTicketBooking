using System.ComponentModel.DataAnnotations;
namespace MovieTicketBooking.DTOs.Seats
{
    public class BulkCreateSeatsRequest
    {
        [Required]
        [MinLength(1)]
        public List<CreateSeatRequest> Seats { get; set; } = new();
    }
}