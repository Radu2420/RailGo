using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Options;
using QRCoder;
using RailGo.Models;

namespace RailGo.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendTicketEmailAsync(Booking booking)
        {
            if (booking == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(booking.PassengerEmail))
            {
                return;
            }

            var tickets = booking.Tickets ?? new List<Ticket>();

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = $"Bilet RailGo - {booking.BookingCode}",
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(new MailAddress(booking.PassengerEmail, booking.PassengerName));

            var htmlBody = BuildEmailBody(booking, tickets);

            var htmlView = AlternateView.CreateAlternateViewFromString(
                htmlBody,
                Encoding.UTF8,
                MediaTypeNames.Text.Html
            );

            foreach (var ticket in tickets)
            {
                var qrText = !string.IsNullOrWhiteSpace(ticket.QrCodeText)
                    ? ticket.QrCodeText
                    : BuildQrText(booking, ticket);

                var qrBytes = GenerateQrCodeBytes(qrText);

                var qrResource = new LinkedResource(
                    new MemoryStream(qrBytes),
                    MediaTypeNames.Image.Png
                );

                qrResource.ContentId = GetQrContentId(ticket);
                qrResource.TransferEncoding = TransferEncoding.Base64;
                qrResource.ContentType.Name = $"QR-{ticket.TicketCode}.png";

                htmlView.LinkedResources.Add(qrResource);
            }

            message.AlternateViews.Add(htmlView);

            using var smtpClient = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            await smtpClient.SendMailAsync(message);
        }

        private string BuildEmailBody(Booking booking, List<Ticket> tickets)
        {
            var ticketsHtml = new StringBuilder();

            foreach (var ticket in tickets)
            {
                var trip = ticket.Trip;
                var train = trip?.Train;
                var route = trip?.RailRoute;

                var departureStation = route?.DepartureStation?.Name ?? "-";
                var arrivalStation = route?.ArrivalStation?.Name ?? "-";

                var trainName = train != null
                    ? $"{train.Name} ({train.TrainCode})"
                    : "-";

                var departureTime = trip != null
                    ? trip.DepartureTime.ToString("dd.MM.yyyy HH:mm")
                    : "-";

                var arrivalTime = trip != null
                    ? trip.ArrivalTime.ToString("dd.MM.yyyy HH:mm")
                    : "-";

                var directionLabel = ticket.DirectionType == "Intors" || ticket.DirectionType == "Întors"
                    ? "Întors"
                    : "Dus";

                var qrCid = GetQrContentId(ticket);

                ticketsHtml.Append($@"
                    <div style=""margin-top:26px;border:1px solid #e5e7eb;border-radius:18px;overflow:hidden;background:#ffffff;"">
                        <div style=""background:#111827;padding:22px 24px;color:#ffffff;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td>
                                        <div style=""font-size:24px;font-weight:800;margin-bottom:6px;"">RailGo Ticket</div>
                                        <div style=""font-size:13px;color:#d1d5db;"">{departureStation} → {arrivalStation}</div>
                                    </td>
                                    <td align=""right"">
                                        <span style=""background:#ff1744;color:#ffffff;padding:8px 14px;border-radius:999px;font-size:12px;font-weight:700;"">{directionLabel}</span>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div style=""padding:22px 24px;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td style=""width:66%;vertical-align:top;padding-right:18px;"">
                                        {InfoBox("COD BILET", ticket.TicketCode)}
                                        {InfoBox("TREN", trainName)}
                                        {InfoBox("PLECARE", departureTime)}
                                        {InfoBox("SOSIRE", arrivalTime)}
                                        {InfoBox("VAGON / LOC", $"Vagon {ticket.CarriageNumber}, loc {ticket.SeatNumber}")}
                                        {InfoBox("CLASĂ", ticket.SeatClass)}
                                        {InfoBox("PREȚ", $"{ticket.Price:0.00} lei")}
                                    </td>

                                    <td style=""width:34%;vertical-align:top;"">
                                        <div style=""border:1px solid #e5e7eb;border-radius:16px;padding:18px;text-align:center;background:#f9fafb;"">
                                            <div style=""font-size:17px;font-weight:800;color:#111827;margin-bottom:12px;"">Cod QR</div>
                                            <img src=""cid:{qrCid}"" width=""150"" height=""150"" alt=""Cod QR bilet"" style=""display:block;width:150px;height:150px;margin:0 auto;border:0;"" />
                                            <div style=""font-size:11px;color:#6b7280;margin-top:12px;line-height:1.4;"">Cod unic pentru validarea biletului.</div>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>");
            }

            var passengerType = string.IsNullOrWhiteSpace(booking.PassengerType)
                ? "Adult"
                : booking.PassengerType;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Bilet RailGo</title>
</head>
<body style=""margin:0;padding:0;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;color:#111827;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f4f6;padding:28px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""640"" cellpadding=""0"" cellspacing=""0"" style=""width:640px;max-width:94%;background:#ffffff;border-radius:22px;overflow:hidden;border:1px solid #e5e7eb;"">
                    <tr>
                        <td style=""background:#111827;padding:34px 34px 40px;color:#ffffff;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td>
                                        <span style=""display:inline-block;background:#ff1744;color:#ffffff;border-radius:12px;padding:10px 12px;font-weight:800;font-size:14px;margin-right:10px;"">RG</span>
                                        <span style=""font-size:18px;font-weight:800;vertical-align:middle;"">RailGo</span>
                                    </td>
                                    <td align=""right"" style=""font-size:13px;color:#d1d5db;font-weight:700;"">Bilet digital</td>
                                </tr>
                            </table>

                            <div style=""font-size:38px;line-height:1.1;font-weight:900;margin-top:34px;"">Rezervare confirmată</div>
                            <div style=""font-size:15px;line-height:1.6;color:#e5e7eb;margin-top:12px;"">
                                Bună, <strong>{booking.PassengerName}</strong>. Biletul tău digital a fost generat și este pregătit pentru validare.
                            </div>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:26px 32px 34px;background:#ffffff;"">
                            <div style=""border:1px solid #e5e7eb;border-radius:16px;padding:18px 20px;margin-bottom:20px;background:#f9fafb;"">
                                <div style=""font-size:12px;color:#6b7280;font-weight:800;letter-spacing:.04em;"">COD REZERVARE</div>
                                <div style=""font-size:18px;font-weight:900;color:#111827;margin-top:6px;"">{booking.BookingCode}</div>
                            </div>

                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom:10px;"">
                                <tr>
                                    <td style=""width:50%;padding:6px;"">
                                        {SmallInfoBox("PASAGER", booking.PassengerName)}
                                    </td>
                                    <td style=""width:50%;padding:6px;"">
                                        {SmallInfoBox("TIP PASAGER", passengerType)}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""width:50%;padding:6px;"">
                                        {SmallInfoBox("TIP BILET", booking.TicketType)}
                                    </td>
                                    <td style=""width:50%;padding:6px;"">
                                        {SmallInfoBox("TOTAL PLATĂ SIMULATĂ", $"{booking.TotalPrice:0.00} lei")}
                                    </td>
                                </tr>
                            </table>

                            {ticketsHtml}

                            <div style=""margin-top:26px;padding:18px 20px;background:#f9fafb;border-radius:16px;border:1px solid #e5e7eb;color:#4b5563;font-size:13px;line-height:1.6;"">
                                Plata este simulată. Acest email este generat automat de aplicația RailGo.
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string InfoBox(string label, string value)
        {
            return $@"
                <div style=""border:1px solid #e5e7eb;border-radius:14px;padding:13px 15px;margin-bottom:10px;background:#ffffff;"">
                    <div style=""font-size:11px;font-weight:800;color:#6b7280;letter-spacing:.04em;"">{label}</div>
                    <div style=""font-size:15px;font-weight:800;color:#111827;margin-top:6px;"">{value}</div>
                </div>";
        }

        private string SmallInfoBox(string label, string value)
        {
            return $@"
                <div style=""border:1px solid #e5e7eb;border-radius:14px;padding:15px 16px;background:#ffffff;"">
                    <div style=""font-size:11px;font-weight:800;color:#6b7280;letter-spacing:.04em;"">{label}</div>
                    <div style=""font-size:15px;font-weight:800;color:#111827;margin-top:6px;"">{value}</div>
                </div>";
        }

        private byte[] GenerateQrCodeBytes(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }

        private string BuildQrText(Booking booking, Ticket ticket)
        {
            var trip = ticket.Trip;
            var train = trip?.Train;

            var trainText = train != null
                ? $"{train.Name} ({train.TrainCode})"
                : "-";

            return
                $"RailGo Ticket\n" +
                $"Rezervare: {booking.BookingCode}\n" +
                $"Bilet: {ticket.TicketCode}\n" +
                $"Pasager: {booking.PassengerName}\n" +
                $"Tip bilet: {booking.TicketType}\n" +
                $"Directie: {ticket.DirectionType}\n" +
                $"Tren: {trainText}\n" +
                $"Vagon: {ticket.CarriageNumber}\n" +
                $"Loc: {ticket.SeatNumber}\n" +
                $"Clasa: {ticket.SeatClass}\n" +
                $"Pret: {ticket.Price:0.00} lei";
        }

        private string GetQrContentId(Ticket ticket)
        {
            return $"qr-{ticket.Id}-{ticket.TicketCode}@railgo";
        }
    }
}