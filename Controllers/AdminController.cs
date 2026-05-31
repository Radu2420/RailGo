using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailGo.Data;
using RailGo.Models;
using RailGo.ViewModels;

namespace RailGo.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "admin")
            {
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.ErrorMessage = "Date de autentificare incorecte.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Trip)
                        .ThenInclude(t => t!.Train)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Trip)
                        .ThenInclude(t => t!.RailRoute)
                            .ThenInclude(r => r!.DepartureStation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Trip)
                        .ThenInclude(t => t!.RailRoute)
                            .ThenInclude(r => r!.ArrivalStation)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var trips = await _context.Trips
                .Include(t => t.Train)
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.DepartureStation)
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.ArrivalStation)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();

            var tickets = bookings
                .SelectMany(b => b.Tickets)
                .ToList();

            var tripRows = new List<TripProfitabilityViewModel>();

            foreach (var trip in trips)
            {
                var tripTickets = tickets
                    .Where(t => t.TripId == trip.Id)
                    .ToList();

                var train = trip.Train;
                var route = trip.RailRoute;

                var carriageCount = train?.CarriageCount ?? 1;
                var seatsPerCarriage = train?.SeatsPerCarriage ?? 40;
                var capacity = carriageCount * seatsPerCarriage;

                if (capacity <= 0)
                {
                    capacity = 1;
                }

                var ticketsSold = tripTickets.Count;
                var revenue = tripTickets.Sum(t => t.Price);
                var occupancyRate = Math.Round((decimal)ticketsSold / capacity * 100m, 2);

                var estimatedCost = CalculateEstimatedTripCost(trip, capacity);
                var estimatedProfit = revenue - estimatedCost;

                var maxRevenue = capacity * trip.Price;
                var breakEvenOccupancyRate = maxRevenue > 0
                    ? Math.Round(estimatedCost / maxRevenue * 100m, 2)
                    : 0m;

                var status = "Profitabilă";
                var cssClass = "profit";

                if (estimatedProfit < 0)
                {
                    status = "Neprofitabilă";
                    cssClass = "loss";
                }
                else if (occupancyRate < breakEvenOccupancyRate + 10)
                {
                    status = "Risc";
                    cssClass = "risk";
                }

                tripRows.Add(new TripProfitabilityViewModel
                {
                    TripId = trip.Id,
                    RouteName = $"{route?.DepartureStation?.Name} → {route?.ArrivalStation?.Name}",
                    TrainName = train?.Name ?? "N/A",
                    TrainCode = train?.TrainCode ?? "N/A",
                    DepartureTime = trip.DepartureTime,
                    Capacity = capacity,
                    TicketsSold = ticketsSold,
                    OccupancyRate = occupancyRate,
                    Revenue = revenue,
                    EstimatedCost = estimatedCost,
                    EstimatedProfit = estimatedProfit,
                    BreakEvenOccupancyRate = breakEvenOccupancyRate,
                    Status = status,
                    StatusCssClass = cssClass
                });
            }

            var popularRoutes = tickets
                .Where(t => t.Trip?.RailRoute != null)
                .GroupBy(t => $"{t.Trip!.RailRoute!.DepartureStation!.Name} → {t.Trip!.RailRoute!.ArrivalStation!.Name}")
                .Select(g => new RoutePopularityViewModel
                {
                    RouteName = g.Key,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .OrderByDescending(r => r.TicketsSold)
                .ThenByDescending(r => r.Revenue)
                .Take(5)
                .ToList();

            var dailyRevenue = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new DailyRevenueViewModel
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.TotalPrice),
                    TicketsSold = g.Sum(b => b.Tickets.Count)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var totalRevenue = bookings.Sum(b => b.TotalPrice);
            var totalEstimatedProfit = tripRows.Sum(t => t.EstimatedProfit);
            var averageOccupancyRate = tripRows.Any()
                ? Math.Round(tripRows.Average(t => t.OccupancyRate), 2)
                : 0m;

            var model = new AdminDashboardViewModel
            {
                TotalBookings = bookings.Count,
                TotalTickets = tickets.Count,
                TotalRevenue = totalRevenue,
                TotalEstimatedProfit = totalEstimatedProfit,
                AverageOccupancyRate = averageOccupancyRate,
                NonProfitableTripsCount = tripRows.Count(t => t.StatusCssClass == "loss"),
                RiskTripsCount = tripRows.Count(t => t.StatusCssClass == "risk"),
                TripProfitabilityRows = tripRows
                    .OrderBy(t => t.EstimatedProfit)
                    .ToList(),
                PopularRoutes = popularRoutes,
                DailyRevenue = dailyRevenue,
                RecentBookings = bookings
                    .Take(6)
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction(nameof(Login));
        }

        private decimal CalculateEstimatedTripCost(Trip trip, int capacity)
        {
            var baseOperationalCost = 1200m;
            var seatOperationalCost = capacity * 8m;
            var routeDuration = trip.ArrivalTime - trip.DepartureTime;
            var durationCost = Math.Max(1, (decimal)routeDuration.TotalHours) * 450m;

            return Math.Round(baseOperationalCost + seatOperationalCost + durationCost, 2);
        }
    }
}