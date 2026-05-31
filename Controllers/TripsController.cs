using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RailGo.Data;
using RailGo.Models;
using RailGo.ViewModels;

namespace RailGo.Controllers
{
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var model = new TripSearchViewModel
            {
                TravelDate = DateTime.Today,
                ReturnDate = DateTime.Today,
                TicketType = "Dus",
                SelectedClass = "Clasa a II-a",
                HasSearched = false
            };

            await PopulateStationLists(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(TripSearchViewModel model)
        {
            model.HasSearched = true;
            model.TicketType = NormalizeTicketType(model.TicketType);

            await PopulateStationLists(model);

            if (!model.DepartureStationId.HasValue)
            {
                ModelState.AddModelError(nameof(model.DepartureStationId), "Alege stația de plecare.");
            }

            if (!model.ArrivalStationId.HasValue)
            {
                ModelState.AddModelError(nameof(model.ArrivalStationId), "Alege stația de sosire.");
            }

            if (model.DepartureStationId.HasValue &&
                model.ArrivalStationId.HasValue &&
                model.DepartureStationId.Value == model.ArrivalStationId.Value)
            {
                ModelState.AddModelError(nameof(model.ArrivalStationId), "Stația de sosire trebuie să fie diferită de stația de plecare.");
            }

            if (IsRoundTrip(model.TicketType))
            {
                if (!model.ReturnDate.HasValue)
                {
                    ModelState.AddModelError(nameof(model.ReturnDate), "Alege data întoarcerii.");
                }
                else if (model.ReturnDate.Value.Date < model.TravelDate.Date)
                {
                    ModelState.AddModelError(nameof(model.ReturnDate), "Data întoarcerii nu poate fi înainte de data plecării.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Trips = new List<Trip>();
                model.Results = new List<Trip>();
                model.ReturnTrips = new List<Trip>();
                model.RoundTripResults = new List<RoundTripSearchResultViewModel>();

                return View(model);
            }

            var departureStart = model.TravelDate.Date;
            var departureEnd = departureStart.AddDays(1);

            var outboundTrips = await _context.Trips
                .Include(t => t.Train)
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.DepartureStation)
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.ArrivalStation)
                .Where(t =>
                    t.RailRoute != null &&
                    t.RailRoute.DepartureStationId == model.DepartureStationId!.Value &&
                    t.RailRoute.ArrivalStationId == model.ArrivalStationId!.Value &&
                    t.DepartureTime >= departureStart &&
                    t.DepartureTime < departureEnd)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();

            model.Trips = outboundTrips;
            model.Results = outboundTrips;

            model.ReturnTrips = new List<Trip>();
            model.RoundTripResults = new List<RoundTripSearchResultViewModel>();

            if (IsRoundTrip(model.TicketType) && model.ReturnDate.HasValue)
            {
                var returnStart = model.ReturnDate.Value.Date;
                var returnEnd = returnStart.AddDays(1);

                var returnTrips = await _context.Trips
                    .Include(t => t.Train)
                    .Include(t => t.RailRoute)
                        .ThenInclude(r => r!.DepartureStation)
                    .Include(t => t.RailRoute)
                        .ThenInclude(r => r!.ArrivalStation)
                    .Where(t =>
                        t.RailRoute != null &&
                        t.RailRoute.DepartureStationId == model.ArrivalStationId!.Value &&
                        t.RailRoute.ArrivalStationId == model.DepartureStationId!.Value &&
                        t.DepartureTime >= returnStart &&
                        t.DepartureTime < returnEnd)
                    .OrderBy(t => t.DepartureTime)
                    .ToListAsync();

                model.ReturnTrips = returnTrips;

                foreach (var outbound in outboundTrips)
                {
                    foreach (var returnTrip in returnTrips)
                    {
                        model.RoundTripResults.Add(new RoundTripSearchResultViewModel
                        {
                            OutboundTrip = outbound,
                            ReturnTrip = returnTrip,
                            OutboundPrice = GetClassPrice(outbound.Price, model.SelectedClass),
                            ReturnPrice = GetClassPrice(returnTrip.Price, model.SelectedClass),
                            TotalPrice = GetClassPrice(outbound.Price, model.SelectedClass) + GetClassPrice(returnTrip.Price, model.SelectedClass)
                        });
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetArrivalStations(int departureStationId)
        {
            if (departureStationId <= 0)
            {
                return Json(new List<object>());
            }

            var arrivalStations = await _context.Trips
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.ArrivalStation)
                .Where(t =>
                    t.RailRoute != null &&
                    t.RailRoute.DepartureStationId == departureStationId &&
                    t.RailRoute.ArrivalStation != null)
                .Select(t => new ArrivalStationDto
                {
                    Id = t.RailRoute!.ArrivalStation!.Id,
                    Name = string.IsNullOrWhiteSpace(t.RailRoute.ArrivalStation.City)
                        ? t.RailRoute.ArrivalStation.Name
                        : t.RailRoute.ArrivalStation.Name + " (" + t.RailRoute.ArrivalStation.City + ")"
                })
                .Distinct()
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Json(arrivalStations.Select(s => new
            {
                id = s.Id,
                name = s.Name
            }));
        }

        private async Task PopulateStationLists(TripSearchViewModel model)
        {
            model.TicketType = NormalizeTicketType(model.TicketType);

            model.Stations = await _context.Stations
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(s.City)
                        ? s.Name
                        : s.Name + " (" + s.City + ")",
                    Selected = model.DepartureStationId.HasValue && model.DepartureStationId.Value == s.Id
                })
                .ToListAsync();

            if (model.DepartureStationId.HasValue)
            {
                var arrivalStations = await _context.Trips
                    .Include(t => t.RailRoute)
                        .ThenInclude(r => r!.ArrivalStation)
                    .Where(t =>
                        t.RailRoute != null &&
                        t.RailRoute.DepartureStationId == model.DepartureStationId.Value &&
                        t.RailRoute.ArrivalStation != null)
                    .Select(t => new ArrivalStationDto
                    {
                        Id = t.RailRoute!.ArrivalStation!.Id,
                        Name = string.IsNullOrWhiteSpace(t.RailRoute.ArrivalStation.City)
                            ? t.RailRoute.ArrivalStation.Name
                            : t.RailRoute.ArrivalStation.Name + " (" + t.RailRoute.ArrivalStation.City + ")"
                    })
                    .Distinct()
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                model.ArrivalStations = arrivalStations
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name,
                        Selected = model.ArrivalStationId.HasValue && model.ArrivalStationId.Value == s.Id
                    })
                    .ToList();
            }
            else
            {
                model.ArrivalStations = new List<SelectListItem>();
            }
        }

        private static bool IsRoundTrip(string? ticketType)
        {
            var normalized = NormalizeTicketType(ticketType);
            return normalized == "Dus-întors";
        }

        private static string NormalizeTicketType(string? ticketType)
        {
            if (string.IsNullOrWhiteSpace(ticketType))
            {
                return "Dus";
            }

            var value = ticketType.Trim();

            if (value.Equals("Dus-Intors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus-întors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus-intors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus întors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus intors", StringComparison.OrdinalIgnoreCase))
            {
                return "Dus-întors";
            }

            return "Dus";
        }

        private decimal GetClassPrice(decimal basePrice, string selectedClass)
        {
            return selectedClass == "Clasa I"
                ? Math.Round(basePrice * 1.25m, 2)
                : basePrice;
        }

        private class ArrivalStationDto : IEquatable<ArrivalStationDto>
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public bool Equals(ArrivalStationDto? other)
            {
                if (other == null)
                {
                    return false;
                }

                return Id == other.Id;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as ArrivalStationDto);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
    }
}