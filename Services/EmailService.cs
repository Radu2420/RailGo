using System.Net;
using System.Net.Mail;
using System.Text;
using QRCoder;
using RailGo.Models;

namespace RailGo.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendTicketEmailAsync(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.PassengerEmail))
            {
                return;
            }

            var subject = $"Bilet RailGo - {booking.BookingCode}";
            var body = BuildEmailBody(booking);

            var senderEmail = GetSetting("SenderEmail", "Username", "Email");
            var senderName = GetSetting("SenderName") ?? "RailGo";
            var username = GetSetting("Username") ?? senderEmail;
            var password = GetSetting("Password");

            var smtpServer = GetSetting("SmtpServer", "Host") ?? "smtp.gmail.com";
            var smtpPort = GetIntSetting("SmtpPort", "Port", 587);
            var enableSsl = GetBoolSetting("EnableSsl", "UseSsl", true);

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException("Lipsește EmailSettings:SenderEmail în appsettings.json.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(booking.PassengerEmail, booking.PassengerName));

            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                smtpClient.Credentials = new NetworkCredential(username, password);
            }

            await smtpClient.SendMailAsync(message);
        }

        private string BuildEmailBody(Booking booking)
        {
            var passengerName = Encode(booking.PassengerName);
            var bookingCode = Encode(booking.BookingCode);
            var ticketType = Encode(booking.TicketType);
            var passengerType = Encode(booking.PassengerType);
            var totalPrice = booking.TotalPrice.ToString("0.00");
            var createdAt = booking.CreatedAt.ToString("dd.MM.yyyy HH:mm");

            var tickets = booking.Tickets
                .OrderBy(t => t.DirectionType == "Dus" ? 0 : 1)
                .ThenBy(t => t.IssuedAt)
                .ToList();

            var ticketBlocks = new StringBuilder();

            foreach (var ticket in tickets)
            {
                ticketBlocks.Append(BuildTicketBlock(booking, ticket));
            }

            return $@"
<!doctype html>
<html lang=""ro"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Bilet RailGo</title>
</head>
<body style=""margin:0;padding:0;background:#f3f5f9;font-family:Arial,Helvetica,sans-serif;color:#111827;"">
    <div style=""display:none;max-height:0;overflow:hidden;color:transparent;"">
        Biletul RailGo pentru rezervarea {bookingCode} a fost generat cu succes.
    </div>

    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f5f9;margin:0;padding:0;"">
        <tr>
            <td align=""center"" style=""padding:28px 14px;"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:760px;background:#ffffff;border-radius:28px;overflow:hidden;border:1px solid #e5e7eb;box-shadow:0 18px 50px rgba(15,23,42,0.10);"">
                    <tr>
                        <td style=""padding:34px 34px 44px;background:linear-gradient(135deg,#111827,#1f2937);color:#ffffff;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td style=""vertical-align:middle;"">
                                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"">
                                            <tr>
                                                <td style=""width:42px;height:42px;background:#ef233c;border-radius:13px;text-align:center;vertical-align:middle;color:#ffffff;font-weight:900;font-size:14px;"">
                                                    RG
                                                </td>
                                                <td style=""padding-left:12px;color:#ffffff;font-weight:900;font-size:19px;"">
                                                    RailGo
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td align=""right"" style=""color:#d1d5db;font-size:13px;font-weight:700;"">
                                        Bilet digital
                                    </td>
                                </tr>
                            </table>

                            <h1 style=""margin:30px 0 10px;font-size:42px;line-height:1.02;letter-spacing:-1.5px;color:#ffffff;font-weight:900;"">
                                Rezervare confirmată
                            </h1>

                            <p style=""margin:0;color:#d1d5db;font-size:16px;line-height:1.55;font-weight:700;"">
                                Bună, {passengerName}. Biletul tău digital a fost generat și este pregătit pentru validare.
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:26px 34px 10px;background:#ffffff;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td style=""padding:16px;border:1px solid #e5e7eb;border-radius:18px;background:#f9fafb;"">
                                        <div style=""font-size:11px;letter-spacing:.6px;text-transform:uppercase;color:#6b7280;font-weight:900;margin-bottom:6px;"">
                                            Cod rezervare
                                        </div>
                                        <div style=""font-size:17px;color:#111827;font-weight:900;word-break:break-word;"">
                                            {bookingCode}
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:8px 34px 18px;background:#ffffff;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td width=""50%"" style=""padding:8px;"">
                                        <div style=""padding:15px;border:1px solid #e5e7eb;border-radius:16px;background:#ffffff;"">
                                            <div style=""font-size:11px;letter-spacing:.6px;text-transform:uppercase;color:#6b7280;font-weight:900;margin-bottom:6px;"">Pasager</div>
                                            <div style=""font-size:15px;color:#111827;font-weight:900;"">{passengerName}</div>
                                        </div>
                                    </td>
                                    <td width=""50%"" style=""padding:8px;"">
                                        <div style=""padding:15px;border:1px solid #e5e7eb;border-radius:16px;background:#ffffff;"">
                                            <div style=""font-size:11px;letter-spacing:.6px;text-transform:uppercase;color:#6b7280;font-weight:900;margin-bottom:6px;"">Tip pasager</div>
                                            <div style=""font-size:15px;color:#111827;font-weight:900;"">{passengerType}</div>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td width=""50%"" style=""padding:8px;"">
                                        <div style=""padding:15px;border:1px solid #e5e7eb;border-radius:16px;background:#ffffff;"">
                                            <div style=""font-size:11px;letter-spacing:.6px;text-transform:uppercase;color:#6b7280;font-weight:900;margin-bottom:6px;"">Tip bilet</div>
                                            <div style=""font-size:15px;color:#111827;font-weight:900;"">{ticketType}</div>
                                        </div>
                                    </td>
                                    <td width=""50%"" style=""padding:8px;"">
                                        <div style=""padding:15px;border:1px solid #e5e7eb;border-radius:16px;background:#ffffff;"">
                                            <div style=""font-size:11px;letter-spacing:.6px;text-transform:uppercase;color:#6b7280;font-weight:900;margin-bottom:6px;"">Total plată simulată</div>
                                            <div style=""font-size:15px;color:#111827;font-weight:900;"">{totalPrice} lei</div>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    {ticketBlocks}

                    <tr>
                        <td style=""padding:24px 34px 32px;background:#ffffff;"">
                            <div style=""padding:18px 20px;border-radius:18px;background:#eff6ff;border:1px solid #bfdbfe;color:#1e3a8a;font-size:14px;line-height:1.55;font-weight:700;"">
                                Plata este simulată. Emailul a fost generat automat de aplicația RailGo la data de {createdAt}.
                            </div>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:20px 34px;background:#111827;color:#9ca3af;font-size:12px;line-height:1.6;"">
                            <strong style=""color:#ffffff;"">RailGo</strong><br>
                            Aplicație demonstrativă pentru rezervarea biletelor feroviare.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string BuildTicketBlock(Booking booking, Ticket ticket)
        {
            var trip = ticket.Trip;
            var route = trip?.RailRoute;

            var departureStation = Encode(route?.DepartureStation?.Name);
            var arrivalStation = Encode(route?.ArrivalStation?.Name);
            var trainName = Encode(trip?.Train?.Name);
            var trainCode = Encode(trip?.Train?.TrainCode);
            var ticketCode = Encode(ticket.TicketCode);
            var direction = Encode(ticket.DirectionType);
            var seatClass = Encode(ticket.SeatClass);
            var departureTime = trip != null ? trip.DepartureTime.ToString("dd.MM.yyyy HH:mm") : "-";
            var arrivalTime = trip != null ? trip.ArrivalTime.ToString("dd.MM.yyyy HH:mm") : "-";
            var issuedAt = ticket.IssuedAt.ToString("dd.MM.yyyy HH:mm");
            var price = ticket.Price.ToString("0.00");

            var qrText = string.IsNullOrWhiteSpace(ticket.QrCodeText)
                ? BuildQrFallbackText(booking, ticket)
                : ticket.QrCodeText;

            var qrBase64 = GenerateQrBase64(qrText);

            return $@"
<tr>
    <td style=""padding:8px 34px 22px;background:#ffffff;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e5e7eb;border-radius:22px;overflow:hidden;background:#ffffff;"">
            <tr>
                <td style=""padding:20px 22px;background:#111827;color:#ffffff;"">
                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                        <tr>
                            <td>
                                <div style=""font-size:23px;font-weight:900;letter-spacing:-.6px;color:#ffffff;"">
                                    RailGo Ticket
                                </div>
                                <div style=""margin-top:6px;color:#d1d5db;font-size:13px;font-weight:700;"">
                                    {departureStation} &rarr; {arrivalStation}
                                </div>
                            </td>
                            <td align=""right"">
                                <span style=""display:inline-block;padding:8px 13px;border-radius:999px;background:#ef233c;color:#ffffff;font-size:12px;font-weight:900;"">
                                    {direction}
                                </span>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>

            <tr>
                <td style=""padding:22px;"">
                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                        <tr>
                            <td style=""vertical-align:top;padding-right:18px;"">
                                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    {EmailInfoRow("Cod bilet", ticketCode)}
                                    {EmailInfoRow("Tren", $"{trainName} ({trainCode})")}
                                    {EmailInfoRow("Plecare", departureTime)}
                                    {EmailInfoRow("Sosire", arrivalTime)}
                                    {EmailInfoRow("Vagon / loc", $"Vagon {ticket.CarriageNumber}, loc {ticket.SeatNumber}")}
                                    {EmailInfoRow("Clasă", seatClass)}
                                    {EmailInfoRow("Preț", $"{price} lei")}
                                    {EmailInfoRow("Emis", issuedAt)}
                                </table>
                            </td>

                            <td width=""190"" style=""vertical-align:top;text-align:center;"">
                                <div style=""padding:14px;border:1px solid #e5e7eb;border-radius:18px;background:#f9fafb;"">
                                    <div style=""font-size:16px;font-weight:900;color:#111827;margin-bottom:10px;"">
                                        Cod QR
                                    </div>

                                    <img src=""data:image/png;base64,{qrBase64}""
                                         width=""150""
                                         height=""150""
                                         alt=""Cod QR bilet""
                                         style=""display:block;width:150px;height:150px;margin:0 auto 10px;border:0;"">

                                    <div style=""font-size:11px;line-height:1.35;color:#6b7280;font-weight:700;"">
                                        Cod unic pentru validarea biletului.
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </td>
</tr>";
        }

        private string EmailInfoRow(string label, string value)
        {
            return $@"
<tr>
    <td style=""padding:0 0 10px;"">
        <div style=""padding:13px 14px;border:1px solid #e5e7eb;border-radius:15px;background:#ffffff;"">
            <div style=""font-size:10px;letter-spacing:.6px;text-transform:uppercase;color:#6b7280;font-weight:900;margin-bottom:5px;"">
                {Encode(label)}
            </div>
            <div style=""font-size:14px;color:#111827;font-weight:900;line-height:1.35;word-break:break-word;"">
                {value}
            </div>
        </div>
    </td>
</tr>";
        }

        private string BuildQrFallbackText(Booking booking, Ticket ticket)
        {
            var trip = ticket.Trip;
            var route = $"{trip?.RailRoute?.DepartureStation?.Name} -> {trip?.RailRoute?.ArrivalStation?.Name}";
            var train = $"{trip?.Train?.Name} ({trip?.Train?.TrainCode})";
            var departure = trip?.DepartureTime.ToString("dd.MM.yyyy HH:mm") ?? "";

            return $"RailGo|Booking={booking.BookingCode}|Ticket={ticket.TicketCode}|Passenger={booking.PassengerName}|Direction={ticket.DirectionType}|Route={route}|Train={train}|Departure={departure}|Carriage={ticket.CarriageNumber}|Seat={ticket.SeatNumber}|Class={ticket.SeatClass}|Generated={DateTime.Now:yyyyMMddHHmmss}";
        }

        private string GenerateQrBase64(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var bytes = qrCode.GetGraphic(20);

            return Convert.ToBase64String(bytes);
        }

        private string Encode(string? value)
        {
            return WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "-" : value);
        }

        private string? GetSetting(params string[] keys)
        {
            foreach (var key in keys)
            {
                var value =
                    _configuration[$"EmailSettings:{key}"] ??
                    _configuration[$"Smtp:{key}"] ??
                    _configuration[$"Email:{key}"];

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private int GetIntSetting(string primaryKey, string fallbackKey, int defaultValue)
        {
            var value = GetSetting(primaryKey, fallbackKey);

            if (int.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        private bool GetBoolSetting(string primaryKey, string fallbackKey, bool defaultValue)
        {
            var value = GetSetting(primaryKey, fallbackKey);

            if (bool.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}