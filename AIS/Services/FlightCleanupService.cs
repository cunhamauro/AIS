using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using AIS.Data.Repositories;

namespace AIS.Services
{
    public class FlightCleanupService : BackgroundService
    {
        private readonly ILogger<FlightCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider; // For getting the repository dependency

        public FlightCleanupService(ILogger<FlightCleanupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider; // For obtaining scoped services like the repository
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Flight cleanup service started.");

            // Run until the application is stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Clean up flights
                    await CleanUpFlights();

                    // Log and wait 10 minutes before next execution
                    _logger.LogInformation("Flight cleanup executed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up old flights.");
                }

                // Wait for 10 minutes
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task CleanUpFlights()
        {
            // Create a scope to get the repository
            using (var scope = _serviceProvider.CreateScope())
            {
                var flightRepository = scope.ServiceProvider.GetRequiredService<IFlightRepository>();

                await flightRepository.DeleteOldFlightsAsync();
            }
        }
    }
}
