using BookingService.Domain.Models;
using Messaging.Abstractions.Persistence;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
}
