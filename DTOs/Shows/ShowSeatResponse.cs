namespace MovieTicketBooking.DTOs.Shows
{
    public class ShowSeatResponse
    {
        public int ShowSeatId { get; set; }

        public int ShowId { get; set; }

        public int SeatId { get; set; }

        public string RowName { get; set; } = string.Empty;

        public int SeatNumber { get; set; }

        public string SeatLabel => $"{RowName}{SeatNumber}";

        public string SeatType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}