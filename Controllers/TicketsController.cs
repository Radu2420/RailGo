using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailGo.Data;
using RailGo.ViewModels;

namespace RailGo.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult History()
        {
            var model = new TicketHistoryViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> History(TicketHistoryViewModel model)
        {
            model.HasSearched = true;

            if (string.IsNullOrWhiteSpace(model.PassengerEmail))
            {
                ModelState.AddModelError(nameof(model.PassengerEmail), "Introdu emailul folosit la cumpărarea biletului.");
                model.Bookings = new List<Models.Booking>();
                return View(model);
            }

            model.Bookings = await _context.Bookings
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Trip)
                        .ThenInclude(tr => tr!.RailRoute)
                            .ThenInclude(r => r!.DepartureStation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Trip)
                        .ThenInclude(tr => tr!.RailRoute)
                            .ThenInclude(r => r!.ArrivalStation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Trip)
                        .ThenInclude(tr => tr!.Train)
                .Where(b => b.PassengerEmail.ToLower() == model.PassengerEmail.ToLower())
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(model);
        }
    }
}