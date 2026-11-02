using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Seats;

namespace MovieTicketBooking.Services.Seats
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _repository;

        public SeatService(ISeatRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SeatResponse>> GetAllAsync(int? screenId)
        {
            var seats = await _repository.GetAllAsync(screenId);

            return seats.Select(MapToResponse).ToList();
        }

        public async Task<SeatResponse?> GetByIdAsync(int id)
        {
            var seat = await _repository.GetByIdAsync(id);

            return seat == null ? null : MapToResponse(seat);
        }

        public async Task<List<SeatResponse>> GetByScreenIdAsync(
            int screenId)
        {
            if (!await _repository.ScreenExistsAsync(screenId))
            {
                throw new KeyNotFoundException("Screen not found.");
            }

            var seats = await _repository.GetByScreenIdAsync(screenId);

            return seats.Select(MapToResponse).ToList();
        }

        public async Task<SeatResponse> CreateAsync(
            CreateSeatRequest request)
        {
            var rowName = request.RowName.Trim().ToUpperInvariant();
            var seatType = request.SeatType.Trim();

            if (!await _repository.ScreenExistsAsync(request.ScreenId))
            {
                throw new KeyNotFoundException("Screen not found.");
            }

            if (await _repository.CoordinateExistsAsync(
                request.ScreenId,
                rowName,
                request.SeatNumber))
            {
                throw new InvalidOperationException(
                    "This seat already exists in the specified row and screen.");
            }

            var seat = new Seat
            {
                ScreenId = request.ScreenId,
                RowName = rowName,
                SeatNumber = request.SeatNumber,
                SeatType = seatType
            };

            var created = await _repository.CreateAsync(seat);

            return MapToResponse(created);
        }

        public async Task<List<SeatResponse>> CreateBulkAsync(
            BulkCreateSeatsRequest request)
        {
            if (request.Seats == null || request.Seats.Count == 0)
            {
                throw new ArgumentException(
                    "At least one seat must be provided.");
            }

            var seats = request.Seats.Select(s => new Seat
            {
                ScreenId = s.ScreenId,
                RowName = s.RowName.Trim().ToUpperInvariant(),
                SeatNumber = s.SeatNumber,
                SeatType = s.SeatType.Trim()
            }).ToList();

            var screenIds = seats.Select(s => s.ScreenId)
                .Distinct()
                .ToList();

            foreach (var screenId in screenIds)
            {
                if (!await _repository.ScreenExistsAsync(screenId))
                {
                    throw new KeyNotFoundException(
                        $"Screen {screenId} was not found.");
                }
            }

            var duplicateCoordinates = seats
                .GroupBy(s => new
                {
                    s.ScreenId,
                    s.RowName,
                    s.SeatNumber
                })
                .Any(g => g.Count() > 1);

            if (duplicateCoordinates)
            {
                throw new InvalidOperationException(
                    "The request contains duplicate seat coordinates.");
            }

            foreach (var seat in seats)
            {
                if (await _repository.CoordinateExistsAsync(
                    seat.ScreenId,
                    seat.RowName,
                    seat.SeatNumber))
                {
                    throw new InvalidOperationException(
                        $"Seat {seat.RowName}{seat.SeatNumber} already exists on screen {seat.ScreenId}.");
                }
            }

            var created = await _repository.CreateBulkAsync(seats);

            return created.Select(MapToResponse).ToList();
        }

        public async Task<SeatResponse?> UpdateAsync(
            int id,
            UpdateSeatRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            if (!await _repository.ScreenExistsAsync(request.ScreenId))
            {
                throw new KeyNotFoundException("Screen not found.");
            }

            if (await _repository.HasDependenciesAsync(id))
            {
                throw new InvalidOperationException(
                    "This seat cannot be updated because it is already associated with show or booking records.");
            }

            var rowName = request.RowName.Trim().ToUpperInvariant();

            if (await _repository.CoordinateExistsAsync(
                request.ScreenId,
                rowName,
                request.SeatNumber,
                id))
            {
                throw new InvalidOperationException(
                    "Another seat already uses these coordinates.");
            }

            existing.ScreenId = request.ScreenId;
            existing.RowName = rowName;
            existing.SeatNumber = request.SeatNumber;
            existing.SeatType = request.SeatType.Trim();

            await _repository.UpdateAsync(existing);

            return MapToResponse(existing);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }

        private static SeatResponse MapToResponse(Seat seat)
        {
            return new SeatResponse
            {
                SeatId = seat.SeatId,
                ScreenId = seat.ScreenId,
                RowName = seat.RowName,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType
            };
        }
    }
}