IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Bookings] (
    [Id] int NOT NULL IDENTITY,
    [BookingCode] nvarchar(max) NOT NULL,
    [PassengerName] nvarchar(max) NOT NULL,
    [PassengerEmail] nvarchar(max) NOT NULL,
    [IsStudent] bit NOT NULL,
    [TicketType] nvarchar(max) NOT NULL,
    [TotalPrice] decimal(10,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id])
);

CREATE TABLE [Stations] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [City] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Stations] PRIMARY KEY ([Id])
);

CREATE TABLE [Trains] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [TrainCode] nvarchar(max) NOT NULL,
    [CarriageCount] int NOT NULL,
    [SeatsPerCarriage] int NOT NULL,
    CONSTRAINT [PK_Trains] PRIMARY KEY ([Id])
);

CREATE TABLE [RailRoutes] (
    [Id] int NOT NULL IDENTITY,
    [DepartureStationId] int NOT NULL,
    [ArrivalStationId] int NOT NULL,
    [DistanceKm] int NOT NULL,
    [DurationMinutes] int NOT NULL,
    CONSTRAINT [PK_RailRoutes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RailRoutes_Stations_ArrivalStationId] FOREIGN KEY ([ArrivalStationId]) REFERENCES [Stations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RailRoutes_Stations_DepartureStationId] FOREIGN KEY ([DepartureStationId]) REFERENCES [Stations] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Trips] (
    [Id] int NOT NULL IDENTITY,
    [RailRouteId] int NOT NULL,
    [TrainId] int NOT NULL,
    [DepartureTime] datetime2 NOT NULL,
    [ArrivalTime] datetime2 NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Trips] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Trips_RailRoutes_RailRouteId] FOREIGN KEY ([RailRouteId]) REFERENCES [RailRoutes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Trips_Trains_TrainId] FOREIGN KEY ([TrainId]) REFERENCES [Trains] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Tickets] (
    [Id] int NOT NULL IDENTITY,
    [TicketCode] nvarchar(max) NOT NULL,
    [BookingId] int NOT NULL,
    [TripId] int NOT NULL,
    [DirectionType] nvarchar(max) NOT NULL,
    [CarriageNumber] int NOT NULL,
    [SeatNumber] int NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    [QrCodeText] nvarchar(max) NOT NULL,
    [IssuedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tickets_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tickets_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [Trips] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_RailRoutes_ArrivalStationId] ON [RailRoutes] ([ArrivalStationId]);

CREATE INDEX [IX_RailRoutes_DepartureStationId] ON [RailRoutes] ([DepartureStationId]);

CREATE INDEX [IX_Tickets_BookingId] ON [Tickets] ([BookingId]);

CREATE INDEX [IX_Tickets_TripId] ON [Tickets] ([TripId]);

CREATE INDEX [IX_Trips_RailRouteId] ON [Trips] ([RailRouteId]);

CREATE INDEX [IX_Trips_TrainId] ON [Trips] ([TrainId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260523081649_InitialCreate', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Bookings] ADD [PassengerType] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Bookings] ADD [StudentCardNumber] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Bookings] ADD [University] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260523100608_AddStudentDetailsToBooking', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'Price');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Tickets] ALTER COLUMN [Price] decimal(18,2) NOT NULL;

ALTER TABLE [Tickets] ADD [SeatClass] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260523140528_AddSeatClassToTicket', N'10.0.8');

COMMIT;
GO

