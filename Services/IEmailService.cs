using RailGo.Models;

namespace RailGo.Services
{
    public interface IEmailService
    {
        Task SendTicketEmailAsync(Booking booking);
    }
}