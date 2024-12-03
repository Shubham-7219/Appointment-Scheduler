using Appointment_Scheduler.Data;

namespace Appointment_Scheduler
{
    public class AppointmentCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AppointmentCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var now = DateTime.Now;

                        // Find appointments where EndTime has passed
                        var expiredAppointments = context.Appointment.Where(a => a.EndTime <= now).ToList();

                        if (expiredAppointments.Any())
                        {
                            context.Appointment.RemoveRange(expiredAppointments);
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error (use ILogger or similar)
                    Console.WriteLine($"Error cleaning up appointments: {ex.Message}");
                }

                // Wait for an hour before checking again (or adjust interval as needed)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
