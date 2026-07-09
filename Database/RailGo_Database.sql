-- ============================================================
-- RailGo - Script structură bază de date
-- Sistem informatic pentru rezervarea, emiterea și administrarea
-- biletelor feroviare în cadrul unei companii private de transport
-- ============================================================

-- Entități principale:
-- Bookings   = rezervările făcute de pasageri
-- Stations   = stațiile feroviare
-- Trains     = trenurile disponibile
-- RailRoutes = rutele dintre două stații
-- Trips      = călătoriile efective, pe o rută și cu un tren
-- Tickets    = biletele generate pentru rezervări

-- Relații principale:
-- Stations 1:N RailRoutes, prin DepartureStationId
-- Stations 1:N RailRoutes, prin ArrivalStationId
-- RailRoutes 1:N Trips
-- Trains 1:N Trips
-- Bookings 1:N Tickets
-- Trips 1:N Tickets

SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;

-- ============================================================
-- Bookings
-- Reține rezervarea generală a pasagerului.
-- O rezervare poate avea un bilet pentru călătorie dus
-- sau două bilete pentru o călătorie dus-întors.
-- ============================================================

IF OBJECT_ID(N'[Bookings]', N'U') IS NULL
BEGIN
    CREATE TABLE [Bookings] (
        [Id] int NOT NULL IDENTITY,
        [BookingCode] nvarchar(100) NOT NULL,
        [PassengerName] nvarchar(200) NOT NULL,
        [PassengerEmail] nvarchar(256) NOT NULL,
        [IsStudent] bit NOT NULL,
        [PassengerType] nvarchar(50) NOT NULL DEFAULT N'Adult',
        [University] nvarchar(200) NOT NULL DEFAULT N'',
        [StudentCardNumber] nvarchar(100) NOT NULL DEFAULT N'',
        [TicketType] nvarchar(50) NOT NULL,
        [TotalPrice] decimal(10,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id])
    );
END;

-- Coloane adăugate ulterior în proiect, păstrate pentru compatibilitate
IF COL_LENGTH(N'[Bookings]', N'PassengerType') IS NULL
    ALTER TABLE [Bookings] ADD [PassengerType] nvarchar(50) NOT NULL DEFAULT N'Adult';

IF COL_LENGTH(N'[Bookings]', N'University') IS NULL
    ALTER TABLE [Bookings] ADD [University] nvarchar(200) NOT NULL DEFAULT N'';

IF COL_LENGTH(N'[Bookings]', N'StudentCardNumber') IS NULL
    ALTER TABLE [Bookings] ADD [StudentCardNumber] nvarchar(100) NOT NULL DEFAULT N'';

-- Ajustare pentru cod unic de rezervare
IF COL_LENGTH(N'[Bookings]', N'BookingCode') IS NOT NULL
    ALTER TABLE [Bookings] ALTER COLUMN [BookingCode] nvarchar(100) NOT NULL;

-- ============================================================
-- Stations
-- Reține stațiile folosite la plecare și sosire.
-- Același tabel este folosit atât pentru stația de plecare,
-- cât și pentru stația de sosire din RailRoutes.
-- ============================================================

IF OBJECT_ID(N'[Stations]', N'U') IS NULL
BEGIN
    CREATE TABLE [Stations] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [City] nvarchar(150) NOT NULL,
        CONSTRAINT [PK_Stations] PRIMARY KEY ([Id])
    );
END;

-- ============================================================
-- Trains
-- Reține trenurile și capacitatea lor.
-- Capacitatea unei curse se calculează pe baza:
-- CarriageCount * SeatsPerCarriage
-- ============================================================

IF OBJECT_ID(N'[Trains]', N'U') IS NULL
BEGIN
    CREATE TABLE [Trains] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [TrainCode] nvarchar(100) NOT NULL,
        [CarriageCount] int NOT NULL,
        [SeatsPerCarriage] int NOT NULL,
        CONSTRAINT [PK_Trains] PRIMARY KEY ([Id])
    );
END;

IF COL_LENGTH(N'[Trains]', N'TrainCode') IS NOT NULL
    ALTER TABLE [Trains] ALTER COLUMN [TrainCode] nvarchar(100) NOT NULL;

-- ============================================================
-- RailRoutes
-- Leagă două stații:
-- DepartureStationId = stația de plecare
-- ArrivalStationId   = stația de sosire
-- ============================================================

IF OBJECT_ID(N'[RailRoutes]', N'U') IS NULL
BEGIN
    CREATE TABLE [RailRoutes] (
        [Id] int NOT NULL IDENTITY,
        [DepartureStationId] int NOT NULL,
        [ArrivalStationId] int NOT NULL,
        [DistanceKm] int NOT NULL,
        [DurationMinutes] int NOT NULL,
        CONSTRAINT [PK_RailRoutes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RailRoutes_Stations_DepartureStationId]
            FOREIGN KEY ([DepartureStationId]) REFERENCES [Stations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RailRoutes_Stations_ArrivalStationId]
            FOREIGN KEY ([ArrivalStationId]) REFERENCES [Stations] ([Id]) ON DELETE NO ACTION
    );
END;

-- ============================================================
-- Trips
-- Reține cursele concrete.
-- O rută poate avea mai multe curse, iar un tren poate fi folosit
-- pentru mai multe curse.
-- EstimatedCost este folosit în dashboard pentru analiza
-- profitabilității călătoriilor.
-- ============================================================

IF OBJECT_ID(N'[Trips]', N'U') IS NULL
BEGIN
    CREATE TABLE [Trips] (
        [Id] int NOT NULL IDENTITY,
        [RailRouteId] int NOT NULL,
        [TrainId] int NOT NULL,
        [DepartureTime] datetime2 NOT NULL,
        [ArrivalTime] datetime2 NOT NULL,
        [Price] decimal(10,2) NOT NULL,
        [EstimatedCost] decimal(10,2) NOT NULL DEFAULT 0.00,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Trips] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Trips_RailRoutes_RailRouteId]
            FOREIGN KEY ([RailRouteId]) REFERENCES [RailRoutes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Trips_Trains_TrainId]
            FOREIGN KEY ([TrainId]) REFERENCES [Trains] ([Id]) ON DELETE CASCADE
    );
END;

IF COL_LENGTH(N'[Trips]', N'EstimatedCost') IS NULL
    ALTER TABLE [Trips] ADD [EstimatedCost] decimal(10,2) NOT NULL DEFAULT 0.00;

IF COL_LENGTH(N'[Trips]', N'IsActive') IS NULL
    ALTER TABLE [Trips] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);

-- ============================================================
-- Tickets
-- Reține biletele generate.
-- Fiecare bilet aparține unei rezervări și unei călătorii.
-- Combinația TripId + CarriageNumber + SeatNumber trebuie
-- să fie unică, pentru a nu permite rezervarea aceluiași loc
-- de două ori pe aceeași călătorie.
-- ============================================================

IF OBJECT_ID(N'[Tickets]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tickets] (
        [Id] int NOT NULL IDENTITY,
        [TicketCode] nvarchar(100) NOT NULL,
        [BookingId] int NOT NULL,
        [TripId] int NOT NULL,
        [DirectionType] nvarchar(50) NOT NULL,
        [CarriageNumber] int NOT NULL,
        [SeatNumber] int NOT NULL,
        [SeatClass] nvarchar(50) NOT NULL DEFAULT N'Clasa a II-a',
        [Price] decimal(18,2) NOT NULL,
        [QrCodeText] nvarchar(max) NOT NULL,
        [IssuedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tickets_Bookings_BookingId]
            FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Tickets_Trips_TripId]
            FOREIGN KEY ([TripId]) REFERENCES [Trips] ([Id]) ON DELETE NO ACTION
    );
END;

IF COL_LENGTH(N'[Tickets]', N'SeatClass') IS NULL
    ALTER TABLE [Tickets] ADD [SeatClass] nvarchar(50) NOT NULL DEFAULT N'Clasa a II-a';

IF COL_LENGTH(N'[Tickets]', N'IssuedAt') IS NULL
    ALTER TABLE [Tickets] ADD [IssuedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();

IF COL_LENGTH(N'[Tickets]', N'TicketCode') IS NOT NULL
    ALTER TABLE [Tickets] ALTER COLUMN [TicketCode] nvarchar(100) NOT NULL;

IF COL_LENGTH(N'[Tickets]', N'Price') IS NOT NULL
    ALTER TABLE [Tickets] ALTER COLUMN [Price] decimal(18,2) NOT NULL;

COMMIT;
GO

BEGIN TRANSACTION;

-- ============================================================
-- FOREIGN KEYS
-- Verificări suplimentare, utile dacă scriptul este rulat peste
-- o bază existentă unde tabelele existau deja fără relații clare.
-- ============================================================

IF OBJECT_ID(N'[FK_RailRoutes_Stations_DepartureStationId]', N'F') IS NULL
BEGIN
    ALTER TABLE [RailRoutes]
    ADD CONSTRAINT [FK_RailRoutes_Stations_DepartureStationId]
    FOREIGN KEY ([DepartureStationId]) REFERENCES [Stations] ([Id]) ON DELETE NO ACTION;
END;

IF OBJECT_ID(N'[FK_RailRoutes_Stations_ArrivalStationId]', N'F') IS NULL
BEGIN
    ALTER TABLE [RailRoutes]
    ADD CONSTRAINT [FK_RailRoutes_Stations_ArrivalStationId]
    FOREIGN KEY ([ArrivalStationId]) REFERENCES [Stations] ([Id]) ON DELETE NO ACTION;
END;

IF OBJECT_ID(N'[FK_Trips_RailRoutes_RailRouteId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Trips]
    ADD CONSTRAINT [FK_Trips_RailRoutes_RailRouteId]
    FOREIGN KEY ([RailRouteId]) REFERENCES [RailRoutes] ([Id]) ON DELETE CASCADE;
END;

IF OBJECT_ID(N'[FK_Trips_Trains_TrainId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Trips]
    ADD CONSTRAINT [FK_Trips_Trains_TrainId]
    FOREIGN KEY ([TrainId]) REFERENCES [Trains] ([Id]) ON DELETE CASCADE;
END;

IF OBJECT_ID(N'[FK_Tickets_Bookings_BookingId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Tickets]
    ADD CONSTRAINT [FK_Tickets_Bookings_BookingId]
    FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE;
END;

IF OBJECT_ID(N'[FK_Tickets_Trips_TripId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Tickets]
    ADD CONSTRAINT [FK_Tickets_Trips_TripId]
    FOREIGN KEY ([TripId]) REFERENCES [Trips] ([Id]) ON DELETE NO ACTION;
END;

COMMIT;
GO

BEGIN TRANSACTION;

-- ============================================================
-- INDEXURI
-- Ajută la căutări rapide și arată clar câmpurile folosite
-- în relațiile dintre tabele.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RailRoutes_DepartureStationId')
    CREATE INDEX [IX_RailRoutes_DepartureStationId]
    ON [RailRoutes] ([DepartureStationId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RailRoutes_ArrivalStationId')
    CREATE INDEX [IX_RailRoutes_ArrivalStationId]
    ON [RailRoutes] ([ArrivalStationId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Trips_RailRouteId')
    CREATE INDEX [IX_Trips_RailRouteId]
    ON [Trips] ([RailRouteId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Trips_TrainId')
    CREATE INDEX [IX_Trips_TrainId]
    ON [Trips] ([TrainId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Trips_DepartureTime')
    CREATE INDEX [IX_Trips_DepartureTime]
    ON [Trips] ([DepartureTime]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Trips_IsActive')
    CREATE INDEX [IX_Trips_IsActive]
    ON [Trips] ([IsActive]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tickets_BookingId')
    CREATE INDEX [IX_Tickets_BookingId]
    ON [Tickets] ([BookingId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tickets_TripId')
    CREATE INDEX [IX_Tickets_TripId]
    ON [Tickets] ([TripId]);

-- ============================================================
-- REGULI DE UNICITATE
-- Acestea fac baza de date mai sigură.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bookings_BookingCode_Unique')
    CREATE UNIQUE INDEX [IX_Bookings_BookingCode_Unique]
    ON [Bookings] ([BookingCode]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Trains_TrainCode_Unique')
    CREATE UNIQUE INDEX [IX_Trains_TrainCode_Unique]
    ON [Trains] ([TrainCode]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tickets_TicketCode_Unique')
    CREATE UNIQUE INDEX [IX_Tickets_TicketCode_Unique]
    ON [Tickets] ([TicketCode]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tickets_TripId_CarriageNumber_SeatNumber_Unique')
    CREATE UNIQUE INDEX [IX_Tickets_TripId_CarriageNumber_SeatNumber_Unique]
    ON [Tickets] ([TripId], [CarriageNumber], [SeatNumber]);

COMMIT;
GO

BEGIN TRANSACTION;

-- ============================================================
-- Istoricul migrațiilor folosite în proiect
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523081649_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260523081649_InitialCreate', N'10.0.8');
END;

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523100608_AddStudentDetailsToBooking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260523100608_AddStudentDetailsToBooking', N'10.0.8');
END;

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523140528_AddSeatClassToTicket'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260523140528_AddSeatClassToTicket', N'10.0.8');
END;

COMMIT;
GO