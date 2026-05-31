using RailGo.Models;

namespace RailGo.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }

        public int TotalTickets { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalEstimatedProfit { get; set; }

        public decimal AverageOccupancyRate { get; set; }

        public int NonProfitableTripsCount { get; set; }

        public int RiskTripsCount { get; set; }

        public List<TripProfitabilityViewModel> TripProfitabilityRows { get; set; } = new();

        public List<RoutePopularityViewModel> PopularRoutes { get; set; } = new();

        public List<DailyRevenueViewModel> DailyRevenue { get; set; } = new();

        public List<Booking> RecentBookings { get; set; } = new();
    }

    public class TripProfitabilityViewModel
    {
        public int TripId { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public string TrainName { get; set; } = string.Empty;

        public string TrainCode { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public int Capacity { get; set; }

        public int TicketsSold { get; set; }

        public decimal OccupancyRate { get; set; }

        public decimal Revenue { get; set; }

        public decimal EstimatedCost { get; set; }

        public decimal EstimatedProfit { get; set; }

        public decimal BreakEvenOccupancyRate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;
    }

    public class RoutePopularityViewModel
    {
        public string RouteName { get; set; } = string.Empty;

        public int TicketsSold { get; set; }

        public decimal Revenue { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }

        public decimal Revenue { get; set; }

        public int TicketsSold { get; set; }
    }
}