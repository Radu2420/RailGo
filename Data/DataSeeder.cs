using RailGo.Models;

namespace RailGo.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            var bucuresti = GetOrCreateStation(context, "București Nord", "București");
            var brasov = GetOrCreateStation(context, "Brașov", "Brașov");
            var constanta = GetOrCreateStation(context, "Constanța", "Constanța");
            var cluj = GetOrCreateStation(context, "Cluj-Napoca", "Cluj-Napoca");
            var iasi = GetOrCreateStation(context, "Iași", "Iași");
            var timisoara = GetOrCreateStation(context, "Timișoara Nord", "Timișoara");
            var sibiu = GetOrCreateStation(context, "Sibiu", "Sibiu");
            var oradea = GetOrCreateStation(context, "Oradea", "Oradea");
            var craiova = GetOrCreateStation(context, "Craiova", "Craiova");
            var galati = GetOrCreateStation(context, "Galați", "Galați");

            context.SaveChanges();

            var regional = GetOrCreateTrain(context, "RailGo Regional", "RG303", 3, 40);
            var intercity = GetOrCreateTrain(context, "RailGo InterCity", "RG202", 4, 40);
            var express = GetOrCreateTrain(context, "RailGo Express", "RG101", 5, 40);
            var premium = GetOrCreateTrain(context, "RailGo Premium", "RG500", 6, 40);

            context.SaveChanges();

            GetOrCreateRoute(context, bucuresti, brasov, 166, 160);
            GetOrCreateRoute(context, brasov, bucuresti, 166, 160);

            GetOrCreateRoute(context, bucuresti, constanta, 225, 145);
            GetOrCreateRoute(context, constanta, bucuresti, 225, 145);

            GetOrCreateRoute(context, bucuresti, cluj, 450, 430);
            GetOrCreateRoute(context, cluj, bucuresti, 450, 430);

            GetOrCreateRoute(context, bucuresti, iasi, 410, 390);
            GetOrCreateRoute(context, iasi, bucuresti, 410, 390);

            GetOrCreateRoute(context, bucuresti, timisoara, 530, 520);
            GetOrCreateRoute(context, timisoara, bucuresti, 530, 520);

            GetOrCreateRoute(context, bucuresti, sibiu, 275, 280);
            GetOrCreateRoute(context, sibiu, bucuresti, 275, 280);

            GetOrCreateRoute(context, cluj, oradea, 155, 150);
            GetOrCreateRoute(context, oradea, cluj, 155, 150);

            GetOrCreateRoute(context, cluj, iasi, 390, 430);
            GetOrCreateRoute(context, iasi, cluj, 390, 430);

            GetOrCreateRoute(context, brasov, sibiu, 145, 135);
            GetOrCreateRoute(context, sibiu, brasov, 145, 135);

            GetOrCreateRoute(context, craiova, bucuresti, 230, 210);
            GetOrCreateRoute(context, bucuresti, craiova, 230, 210);

            GetOrCreateRoute(context, galati, bucuresti, 260, 240);
            GetOrCreateRoute(context, bucuresti, galati, 260, 240);

            context.SaveChanges();

            var templates = new List<TripTemplate>
            {
                new TripTemplate(GetRoute(context, bucuresti, brasov), express, 6, 30, 80),
                new TripTemplate(GetRoute(context, bucuresti, brasov), intercity, 9, 00, 95),
                new TripTemplate(GetRoute(context, bucuresti, brasov), regional, 14, 30, 70),
                new TripTemplate(GetRoute(context, bucuresti, brasov), express, 18, 00, 80),

                new TripTemplate(GetRoute(context, brasov, bucuresti), express, 7, 00, 80),
                new TripTemplate(GetRoute(context, brasov, bucuresti), intercity, 12, 00, 95),
                new TripTemplate(GetRoute(context, brasov, bucuresti), express, 18, 00, 80),
                new TripTemplate(GetRoute(context, brasov, bucuresti), regional, 20, 15, 70),

                new TripTemplate(GetRoute(context, bucuresti, constanta), intercity, 8, 00, 95),
                new TripTemplate(GetRoute(context, bucuresti, constanta), express, 11, 30, 110),
                new TripTemplate(GetRoute(context, bucuresti, constanta), regional, 16, 00, 85),

                new TripTemplate(GetRoute(context, constanta, bucuresti), intercity, 7, 30, 95),
                new TripTemplate(GetRoute(context, constanta, bucuresti), express, 13, 00, 110),
                new TripTemplate(GetRoute(context, constanta, bucuresti), regional, 17, 00, 85),

                new TripTemplate(GetRoute(context, bucuresti, cluj), premium, 7, 15, 160),
                new TripTemplate(GetRoute(context, bucuresti, cluj), intercity, 13, 00, 140),
                new TripTemplate(GetRoute(context, cluj, bucuresti), premium, 8, 00, 160),
                new TripTemplate(GetRoute(context, cluj, bucuresti), intercity, 15, 00, 140),

                new TripTemplate(GetRoute(context, bucuresti, iasi), intercity, 7, 45, 130),
                new TripTemplate(GetRoute(context, bucuresti, iasi), premium, 15, 30, 155),
                new TripTemplate(GetRoute(context, iasi, bucuresti), intercity, 8, 15, 130),
                new TripTemplate(GetRoute(context, iasi, bucuresti), premium, 16, 10, 155),

                new TripTemplate(GetRoute(context, bucuresti, timisoara), premium, 6, 50, 180),
                new TripTemplate(GetRoute(context, bucuresti, timisoara), intercity, 12, 20, 155),
                new TripTemplate(GetRoute(context, timisoara, bucuresti), premium, 7, 10, 180),
                new TripTemplate(GetRoute(context, timisoara, bucuresti), intercity, 13, 10, 155),

                new TripTemplate(GetRoute(context, bucuresti, sibiu), express, 9, 20, 115),
                new TripTemplate(GetRoute(context, sibiu, bucuresti), express, 17, 30, 115),

                new TripTemplate(GetRoute(context, cluj, oradea), regional, 10, 00, 65),
                new TripTemplate(GetRoute(context, oradea, cluj), regional, 18, 00, 65),

                new TripTemplate(GetRoute(context, cluj, iasi), intercity, 7, 40, 135),
                new TripTemplate(GetRoute(context, iasi, cluj), intercity, 14, 20, 135),

                new TripTemplate(GetRoute(context, brasov, sibiu), regional, 9, 45, 55),
                new TripTemplate(GetRoute(context, sibiu, brasov), regional, 16, 45, 55),

                new TripTemplate(GetRoute(context, bucuresti, craiova), intercity, 8, 30, 90),
                new TripTemplate(GetRoute(context, craiova, bucuresti), intercity, 17, 20, 90),

                new TripTemplate(GetRoute(context, bucuresti, galati), intercity, 10, 10, 105),
                new TripTemplate(GetRoute(context, galati, bucuresti), intercity, 18, 40, 105)
            };

            SeedTripsForNextDays(context, templates, 45);
        }

        private static Station GetOrCreateStation(ApplicationDbContext context, string name, string city)
        {
            var station = context.Stations.FirstOrDefault(s => s.Name == name);

            if (station == null)
            {
                station = new Station
                {
                    Name = name,
                    City = city
                };

                context.Stations.Add(station);
                context.SaveChanges();
            }
            else
            {
                station.City = city;
                context.Stations.Update(station);
                context.SaveChanges();
            }

            return station;
        }

        private static Train GetOrCreateTrain(ApplicationDbContext context, string name, string trainCode, int carriageCount, int seatsPerCarriage)
        {
            var train = context.Trains.FirstOrDefault(t => t.TrainCode == trainCode);

            if (train == null)
            {
                train = new Train
                {
                    Name = name,
                    TrainCode = trainCode,
                    CarriageCount = carriageCount,
                    SeatsPerCarriage = seatsPerCarriage
                };

                context.Trains.Add(train);
                context.SaveChanges();
            }
            else
            {
                train.Name = name;
                train.CarriageCount = carriageCount;
                train.SeatsPerCarriage = seatsPerCarriage;
                context.Trains.Update(train);
                context.SaveChanges();
            }

            return train;
        }

        private static RailRoute GetOrCreateRoute(ApplicationDbContext context, Station departure, Station arrival, int distanceKm, int durationMinutes)
        {
            var route = context.RailRoutes.FirstOrDefault(r =>
                r.DepartureStationId == departure.Id &&
                r.ArrivalStationId == arrival.Id);

            if (route == null)
            {
                route = new RailRoute
                {
                    DepartureStationId = departure.Id,
                    ArrivalStationId = arrival.Id,
                    DistanceKm = distanceKm,
                    DurationMinutes = durationMinutes
                };

                context.RailRoutes.Add(route);
                context.SaveChanges();
            }
            else
            {
                route.DistanceKm = distanceKm;
                route.DurationMinutes = durationMinutes;
                context.RailRoutes.Update(route);
                context.SaveChanges();
            }

            return route;
        }

        private static RailRoute GetRoute(ApplicationDbContext context, Station departure, Station arrival)
        {
            return context.RailRoutes.First(r =>
                r.DepartureStationId == departure.Id &&
                r.ArrivalStationId == arrival.Id);
        }

        private static void SeedTripsForNextDays(ApplicationDbContext context, List<TripTemplate> templates, int numberOfDays)
        {
            for (int day = 1; day <= numberOfDays; day++)
            {
                var date = DateTime.Today.AddDays(day).Date;

                foreach (var template in templates)
                {
                    var departureTime = date
                        .AddHours(template.DepartureHour)
                        .AddMinutes(template.DepartureMinute);

                    var arrivalTime = departureTime.AddMinutes(Convert.ToDouble(template.Route.DurationMinutes));

                    var alreadyExists = context.Trips.Any(t =>
                        t.RailRouteId == template.Route.Id &&
                        t.TrainId == template.Train.Id &&
                        t.DepartureTime == departureTime);

                    if (!alreadyExists)
                    {
                        var trip = new Trip
                        {
                            RailRouteId = template.Route.Id,
                            TrainId = template.Train.Id,
                            DepartureTime = departureTime,
                            ArrivalTime = arrivalTime,
                            Price = template.Price,
                            IsActive = true
                        };

                        context.Trips.Add(trip);
                    }
                }
            }

            context.SaveChanges();
        }

        private class TripTemplate
        {
            public RailRoute Route { get; set; }

            public Train Train { get; set; }

            public int DepartureHour { get; set; }

            public int DepartureMinute { get; set; }

            public decimal Price { get; set; }

            public TripTemplate(RailRoute route, Train train, int departureHour, int departureMinute, decimal price)
            {
                Route = route;
                Train = train;
                DepartureHour = departureHour;
                DepartureMinute = departureMinute;
                Price = price;
            }
        }
    }
}