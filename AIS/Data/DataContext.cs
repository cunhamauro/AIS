using AIS.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Aircraft> Aircrafts { get; set; }

        public DbSet<Airport> Airports { get; set; }

        public DbSet<Flight> Flights { get; set; }

        //public DbSet<FlightRecord> FlightRecords { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airport>()
                       .HasIndex(a => a.IATA)
                       .IsUnique()
                       .HasDatabaseName("IX_Airports_IATA");

            // Convert Lists to Json to save in database:
            // Converter
            var valueConverter = new ValueConverter<List<string>, string>(
                 v => JsonConvert.SerializeObject(v), // Convert List<string> to JSON string for database persistance
                 v => JsonConvert.DeserializeObject<List<string>>(v) // Convert JSON string back to List<string>
            );

            // Comparer
            var valueComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2), // Compare two lists for equality
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Create a combined hash code for the list
                c => c.ToList() // Clone the list to avoid references to the same list
            );

            // Configure Aircraft
            modelBuilder.Entity<Aircraft>()
                        .Property(x => x.Seats)
                        .HasConversion(valueConverter)
                        .Metadata.SetValueComparer(valueComparer);

            // Configure Flight
            modelBuilder.Entity<Flight>()
                        .Property(x => x.AvailableSeats)
                        .HasConversion(valueConverter)
                        .Metadata.SetValueComparer(valueComparer);

            base.OnModelCreating(modelBuilder);
        }

        //public async Task CreateOrUpdateStoredProcedureAsync()
        //{
        //    var sql = @"
        //                IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = N'StoredProcedureUpdateFlightRecords')
        //                BEGIN
        //                    EXEC sp_executesql N'CREATE PROCEDURE StoredProcedureUpdateFlightRecords
        //                        @FlightId INT,
        //                        @AircraftId INT,
        //                        @OriginId INT,
        //                        @DestinationId INT,
        //                        @Departure DATETIME,
        //                        @Arrival DATETIME,
        //                        @FlightNumber NVARCHAR(50),
        //                        @PassengerCount INT
        //                    AS
        //                    BEGIN
        //                        -- Check if a record already exists
        //                        IF EXISTS (SELECT 1 FROM FlightRecords WHERE Id = @FlightId)
        //                        BEGIN
        //                            -- Update the existing record
        //                            UPDATE FlightRecords
        //                            SET OriginId = @OriginId,
        //                                DestinationId = @DestinationId,
        //                                Departure = @Departure,
        //                                Arrival = @Arrival,
        //                                FlightNumber = @FlightNumber,
        //                                PassengerCount = @PassengerCount
        //                            WHERE Id = @FlightId;
        //                        END
        //                        ELSE
        //                        BEGIN
        //                            -- Enable IDENTITY_INSERT for explicit ID insertion
        //                            SET IDENTITY_INSERT FlightRecords ON;

        //                            -- Insert a new record
        //                            INSERT INTO FlightRecords (Id, AircraftId, OriginId, DestinationId, Departure, Arrival, FlightNumber, PassengerCount)
        //                            VALUES (@FlightId, @AircraftId, @OriginId, @DestinationId, @Departure, @Arrival, @FlightNumber, @PassengerCount);

        //                            -- Disable IDENTITY_INSERT after the insertion
        //                            SET IDENTITY_INSERT FlightRecords OFF;
        //                        END
        //                    END';
        //                END";

        //    await this.Database.ExecuteSqlRawAsync(sql);
        //}

        //public async Task CreateTriggerAsync()
        //{
        //    var sql = @"
        //                IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = N'TriggerUpdateFlightRecords')
        //                BEGIN
        //                    EXEC sp_executesql N'CREATE TRIGGER TriggerUpdateFlightRecords
        //                        ON Flights
        //                        AFTER INSERT, UPDATE
        //                        AS
        //                        BEGIN
        //                            -- Declare variables to hold the flight data
        //                            DECLARE @FlightId INT, @AircraftId INT, @OriginId INT, @DestinationId INT,
        //                                    @Departure DATETIME, @Arrival DATETIME, @FlightNumber NVARCHAR(50),
        //                                    @PassengerCount INT;

        //                            -- Extract data from the inserted or updated flight
        //                            SELECT @FlightId = inserted.Id,
        //                                   @AircraftId = inserted.AircraftId,  
        //                                   @OriginId = inserted.OriginId,      
        //                                   @DestinationId = inserted.DestinationId,
        //                                   @Departure = inserted.Departure,
        //                                   @Arrival = inserted.Arrival,
        //                                   @FlightNumber = inserted.FlightNumber,
        //                                   @PassengerCount = inserted.PassengerCount
        //                            FROM inserted;

        //                            -- Call the stored procedure to upsert the flight record
        //                            EXEC StoredProcedureUpdateFlightRecords @FlightId, @AircraftId, @OriginId, @DestinationId, @Departure, @Arrival, @FlightNumber, @PassengerCount;
        //                        END';
        //                END";

        //    await this.Database.ExecuteSqlRawAsync(sql);
        //}
    }
}
