using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using RailGo.Data;
using RailGo.Models;
using RailGo.Services;
using RailGo.ViewModels;

namespace RailGo.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BookingController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(
            int tripId,
            int? returnTripId,
            string ticketType = "Dus",
            int carriageNumber = 1,
            int? returnCarriageNumber = null)
        {
            ticketType = NormalizeTicketType(ticketType);

            var model = new SeatSelectionViewModel
            {
                TripId = tripId,
                ReturnTripId = returnTripId,
                TicketType = ticketType,
                SelectedCarriage = carriageNumber,
                ReturnSelectedCarriage = returnCarriageNumber ?? carriageNumber,
                PassengerType = "Adult"
            };

            var loaded = await PopulateSeatSelectionModel(model);

            if (!loaded)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(SeatSelectionViewModel model)
        {
            model.TicketType = NormalizeTicketType(model.TicketType);

            var loaded = await PopulateSeatSelectionModel(model);

            if (!loaded)
            {
                return NotFound();
            }

            ModelState.Clear();

            ValidateBookingData(model);

            if (!ModelState.IsValid)
            {
                await PopulateSeatSelectionModel(model);
                return View("SelectSeat", model);
            }

            return View("Payment", model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(SeatSelectionViewModel model)
        {
            model.TicketType = NormalizeTicketType(model.TicketType);

            var loaded = await PopulateSeatSelectionModel(model);

            if (!loaded)
            {
                return NotFound();
            }

            ModelState.Clear();

            ValidateBookingData(model);

            if (!ModelState.IsValid)
            {
                await PopulateSeatSelectionModel(model);
                return View("SelectSeat", model);
            }

            model.PassengerName = model.PassengerName.Trim();
            model.PassengerEmail = model.PassengerEmail.Trim();
            model.PassengerType = string.IsNullOrWhiteSpace(model.PassengerType)
                ? "Adult"
                : model.PassengerType.Trim();

            var isRoundTrip = IsRoundTrip(model.TicketType);

            var booking = new Booking
            {
                BookingCode = GenerateBookingCode(),
                PassengerName = model.PassengerName,
                PassengerEmail = model.PassengerEmail,
                TicketType = model.TicketType,
                PassengerType = model.PassengerType,
                University = model.PassengerType == "Student" ? model.University ?? string.Empty : string.Empty,
                StudentCardNumber = model.PassengerType == "Student" ? model.StudentCardNumber ?? string.Empty : string.Empty,
                TotalPrice = model.TotalPrice,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var outboundTicketCode = GenerateTicketCode();

            var outboundTicket = new Ticket
            {
                BookingId = booking.Id,
                TripId = model.TripId,
                TicketCode = outboundTicketCode,
                DirectionType = "Dus",
                CarriageNumber = model.SelectedCarriage,
                SeatNumber = model.SelectedSeat,
                SeatClass = model.SeatClass,
                Price = model.OutboundFinalPrice,
                IssuedAt = DateTime.Now,
                QrCodeText = GenerateQrText(
                    booking.BookingCode,
                    outboundTicketCode,
                    model.PassengerName,
                    "Dus",
                    model.Trip,
                    model.SelectedCarriage,
                    model.SelectedSeat,
                    model.SeatClass)
            };

            _context.Tickets.Add(outboundTicket);

            if (isRoundTrip &&
                model.ReturnTripId.HasValue &&
                model.ReturnSelectedSeat.HasValue &&
                model.ReturnSelectedCarriage.HasValue)
            {
                var returnTicketCode = GenerateTicketCode();

                var returnTicket = new Ticket
                {
                    BookingId = booking.Id,
                    TripId = model.ReturnTripId.Value,
                    TicketCode = returnTicketCode,
                    DirectionType = "Întors",
                    CarriageNumber = model.ReturnSelectedCarriage.Value,
                    SeatNumber = model.ReturnSelectedSeat.Value,
                    SeatClass = model.ReturnSeatClass,
                    Price = model.ReturnFinalPrice,
                    IssuedAt = DateTime.Now,
                    QrCodeText = GenerateQrText(
                        booking.BookingCode,
                        returnTicketCode,
                        model.PassengerName,
                        "Întors",
                        model.ReturnTrip,
                        model.ReturnSelectedCarriage.Value,
                        model.ReturnSelectedSeat.Value,
                        model.ReturnSeatClass)
                };

                _context.Tickets.Add(returnTicket);
            }

            await _context.SaveChangesAsync();

            var savedBooking = await GetBookingWithDetails(booking.Id);

            if (savedBooking != null)
            {
                try
                {
                    await _emailService.SendTicketEmailAsync(savedBooking);
                    TempData["EmailMessage"] = "Biletul a fost trimis automat pe email.";
                }
                catch
                {
                    TempData["EmailMessage"] = "Biletul a fost generat, dar emailul nu a putut fi trimis automat.";
                }
            }

            return RedirectToAction(nameof(Confirmation), new { bookingId = booking.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int bookingId)
        {
            var booking = await GetBookingWithDetails(bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var qrCodes = new Dictionary<int, string>();

            foreach (var ticket in booking.Tickets)
            {
                qrCodes[ticket.Id] = Convert.ToBase64String(GenerateQrCodeBytes(ticket.QrCodeText));
            }

            var model = new BookingConfirmationViewModel
            {
                Booking = booking,
                TicketQrCodes = qrCodes,
                EmailMessage = TempData["EmailMessage"]?.ToString() ?? string.Empty
            };

            return View(model);
        }

        private void ValidateBookingData(SeatSelectionViewModel model)
        {
            var isRoundTrip = IsRoundTrip(model.TicketType);

            if (string.IsNullOrWhiteSpace(model.PassengerName))
            {
                ModelState.AddModelError(nameof(model.PassengerName), "Completează numele pasagerului.");
            }

            if (string.IsNullOrWhiteSpace(model.PassengerEmail))
            {
                ModelState.AddModelError(nameof(model.PassengerEmail), "Completează adresa de email.");
            }
            else
            {
                try
                {
                    var email = new System.Net.Mail.MailAddress(model.PassengerEmail.Trim());

                    if (!email.Address.Equals(model.PassengerEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(nameof(model.PassengerEmail), "Adresa de email nu este validă.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(nameof(model.PassengerEmail), "Adresa de email nu este validă.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.PassengerType))
            {
                ModelState.AddModelError(nameof(model.PassengerType), "Alege tipul pasagerului.");
            }

            if (model.SelectedCarriage <= 0)
            {
                ModelState.AddModelError(nameof(model.SelectedCarriage), "Alege vagonul pentru călătoria dus.");
            }

            if (model.SelectedSeat <= 0)
            {
                ModelState.AddModelError(nameof(model.SelectedSeat), "Alege locul pentru călătoria dus.");
            }

            if (model.SelectedCarriage > 0 && model.SelectedSeat > 0)
            {
                var outboundStartSeat = ((model.SelectedCarriage - 1) * model.SeatsPerCarriage) + 1;
                var outboundEndSeat = model.SelectedCarriage * model.SeatsPerCarriage;

                if (model.SelectedSeat < outboundStartSeat || model.SelectedSeat > outboundEndSeat)
                {
                    ModelState.AddModelError(nameof(model.SelectedSeat), "Locul ales nu aparține vagonului selectat pentru dus.");
                }
            }

            if (isRoundTrip)
            {
                if (!model.ReturnTripId.HasValue || model.ReturnTripId.Value <= 0)
                {
                    ModelState.AddModelError(nameof(model.ReturnTripId), "Pentru bilet dus-întors trebuie selectat trenul de întoarcere.");
                }

                if (!model.ReturnSelectedCarriage.HasValue || model.ReturnSelectedCarriage.Value <= 0)
                {
                    ModelState.AddModelError(nameof(model.ReturnSelectedCarriage), "Alege vagonul pentru călătoria de întoarcere.");
                }

                if (!model.ReturnSelectedSeat.HasValue || model.ReturnSelectedSeat.Value <= 0)
                {
                    ModelState.AddModelError(nameof(model.ReturnSelectedSeat), "Alege locul pentru călătoria de întoarcere.");
                }

                if (model.ReturnTrip == null)
                {
                    ModelState.AddModelError(nameof(model.ReturnTripId), "Trenul de întoarcere nu a putut fi încărcat.");
                }

                if (model.ReturnSelectedCarriage.HasValue &&
                    model.ReturnSelectedCarriage.Value > 0 &&
                    model.ReturnSelectedSeat.HasValue &&
                    model.ReturnSelectedSeat.Value > 0)
                {
                    var returnStartSeat = ((model.ReturnSelectedCarriage.Value - 1) * model.ReturnSeatsPerCarriage) + 1;
                    var returnEndSeat = model.ReturnSelectedCarriage.Value * model.ReturnSeatsPerCarriage;

                    if (model.ReturnSelectedSeat.Value < returnStartSeat || model.ReturnSelectedSeat.Value > returnEndSeat)
                    {
                        ModelState.AddModelError(nameof(model.ReturnSelectedSeat), "Locul ales nu aparține vagonului selectat pentru întoarcere.");
                    }
                }
            }

            if (model.PassengerType == "Student")
            {
                if (string.IsNullOrWhiteSpace(model.University))
                {
                    ModelState.AddModelError(nameof(model.University), "Pentru student trebuie completată universitatea.");
                }

                if (string.IsNullOrWhiteSpace(model.StudentCardNumber))
                {
                    ModelState.AddModelError(nameof(model.StudentCardNumber), "Pentru student trebuie completat numărul legitimației.");
                }
            }

            if (model.SelectedSeat > 0 && model.SelectedCarriage > 0)
            {
                var outboundSeatTaken = _context.Tickets.Any(t =>
                    t.TripId == model.TripId &&
                    t.CarriageNumber == model.SelectedCarriage &&
                    t.SeatNumber == model.SelectedSeat);

                if (outboundSeatTaken)
                {
                    ModelState.AddModelError(nameof(model.SelectedSeat), "Locul ales pentru dus este deja ocupat. Alege alt loc.");
                }
            }

            if (isRoundTrip &&
                model.ReturnTripId.HasValue &&
                model.ReturnSelectedSeat.HasValue &&
                model.ReturnSelectedCarriage.HasValue &&
                model.ReturnSelectedSeat.Value > 0 &&
                model.ReturnSelectedCarriage.Value > 0)
            {
                var returnSeatTaken = _context.Tickets.Any(t =>
                    t.TripId == model.ReturnTripId.Value &&
                    t.CarriageNumber == model.ReturnSelectedCarriage.Value &&
                    t.SeatNumber == model.ReturnSelectedSeat.Value);

                if (returnSeatTaken)
                {
                    ModelState.AddModelError(nameof(model.ReturnSelectedSeat), "Locul ales pentru întoarcere este deja ocupat. Alege alt loc.");
                }
            }
        }

        private async Task<bool> PopulateSeatSelectionModel(SeatSelectionViewModel model)
        {
            model.TicketType = NormalizeTicketType(model.TicketType);

            var trip = await _context.Trips
                .Include(t => t.Train)
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.DepartureStation)
                .Include(t => t.RailRoute)
                    .ThenInclude(r => r!.ArrivalStation)
                .FirstOrDefaultAsync(t => t.Id == model.TripId);

            if (trip == null)
            {
                return false;
            }

            model.Trip = trip;
            model.CarriageCount = trip.Train?.CarriageCount ?? 1;
            model.SeatsPerCarriage = trip.Train?.SeatsPerCarriage ?? 40;
            model.SelectedCarriage = NormalizeCarriage(model.SelectedCarriage, model.CarriageCount);
            model.SeatClass = GetSeatClass(model.SelectedCarriage);

            model.OutboundNormalPrice = GetClassPrice(trip.Price, model.SelectedCarriage);
            model.OutboundFinalPrice = ApplyPassengerDiscount(model.OutboundNormalPrice, model.PassengerType);

            model.OccupiedSeats = await _context.Tickets
                .Where(t => t.TripId == model.TripId && t.CarriageNumber == model.SelectedCarriage)
                .Select(t => t.SeatNumber)
                .Distinct()
                .ToListAsync();

            if (IsRoundTrip(model.TicketType) && model.ReturnTripId.HasValue)
            {
                var returnTrip = await _context.Trips
                    .Include(t => t.Train)
                    .Include(t => t.RailRoute)
                        .ThenInclude(r => r!.DepartureStation)
                    .Include(t => t.RailRoute)
                        .ThenInclude(r => r!.ArrivalStation)
                    .FirstOrDefaultAsync(t => t.Id == model.ReturnTripId.Value);

                if (returnTrip != null)
                {
                    model.ReturnTrip = returnTrip;
                    model.ReturnCarriageCount = returnTrip.Train?.CarriageCount ?? 1;
                    model.ReturnSeatsPerCarriage = returnTrip.Train?.SeatsPerCarriage ?? 40;

                    model.ReturnSelectedCarriage ??= model.SelectedCarriage;
                    model.ReturnSelectedCarriage = NormalizeCarriage(model.ReturnSelectedCarriage.Value, model.ReturnCarriageCount);
                    model.ReturnSeatClass = GetSeatClass(model.ReturnSelectedCarriage.Value);

                    model.ReturnNormalPrice = GetClassPrice(returnTrip.Price, model.ReturnSelectedCarriage.Value);
                    model.ReturnFinalPrice = ApplyPassengerDiscount(model.ReturnNormalPrice, model.PassengerType);

                    model.ReturnOccupiedSeats = await _context.Tickets
                        .Where(t => t.TripId == model.ReturnTripId.Value &&
                                    t.CarriageNumber == model.ReturnSelectedCarriage.Value)
                        .Select(t => t.SeatNumber)
                        .Distinct()
                        .ToListAsync();
                }
            }

            model.TotalPrice = model.OutboundFinalPrice + model.ReturnFinalPrice;

            return true;
        }

        private async Task<Booking?> GetBookingWithDetails(int bookingId)
        {
            return await _context.Bookings
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
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        private int NormalizeCarriage(int carriageNumber, int carriageCount)
        {
            if (carriageNumber < 1)
            {
                return 1;
            }

            if (carriageNumber > carriageCount)
            {
                return carriageCount;
            }

            return carriageNumber;
        }

        private string GetSeatClass(int carriageNumber)
        {
            return carriageNumber == 1 ? "Clasa I" : "Clasa a II-a";
        }

        private decimal GetClassPrice(decimal basePrice, int carriageNumber)
        {
            return carriageNumber == 1
                ? Math.Round(basePrice * 1.25m, 2)
                : basePrice;
        }

        private decimal ApplyPassengerDiscount(decimal price, string? passengerType)
        {
            return passengerType == "Student"
                ? Math.Round(price * 0.5m, 2)
                : price;
        }

        private bool IsRoundTrip(string? ticketType)
        {
            return NormalizeTicketType(ticketType) == "Dus-întors";
        }

        private string NormalizeTicketType(string? ticketType)
        {
            if (string.IsNullOrWhiteSpace(ticketType))
            {
                return "Dus";
            }

            var value = ticketType.Trim();

            if (value.Equals("Dus-întors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus-Intors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus-intors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus întors", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Dus intors", StringComparison.OrdinalIgnoreCase))
            {
                return "Dus-întors";
            }

            return "Dus";
        }

        private string GenerateBookingCode()
        {
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"RG-BKG-{DateTime.Now:yyyyMMddHHmmss}-{randomPart}";
        }

        private string GenerateTicketCode()
        {
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"RG-TCK-{DateTime.Now:yyyyMMddHHmmss}-{randomPart}";
        }

        private string GenerateQrText(
            string bookingCode,
            string ticketCode,
            string passengerName,
            string direction,
            Trip? trip,
            int carriageNumber,
            int seatNumber,
            string seatClass)
        {
            var route = $"{trip?.RailRoute?.DepartureStation?.Name} -> {trip?.RailRoute?.ArrivalStation?.Name}";
            var train = $"{trip?.Train?.Name} ({trip?.Train?.TrainCode})";
            var departure = trip?.DepartureTime.ToString("dd.MM.yyyy HH:mm") ?? "";

            return $"RailGo|Booking={bookingCode}|Ticket={ticketCode}|Passenger={passengerName}|Direction={direction}|Route={route}|Train={train}|Departure={departure}|Carriage={carriageNumber}|Seat={seatNumber}|Class={seatClass}|Generated={DateTime.Now:yyyyMMddHHmmss}";
        }

        private byte[] GenerateQrCodeBytes(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }
    }
}