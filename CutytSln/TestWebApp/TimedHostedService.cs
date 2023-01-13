namespace TestWebApp
{
    public class TimedHostedService : IHostedService
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private Timer? _timer = null;

        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                var siteLocation = "C:\\home\\site";
                if (Directory.Exists(siteLocation))
                {
                    var files = Directory.GetFiles(siteLocation);

                    foreach (var file in files)
                    {
                        var parts = file.Split('_', '.');

                        foreach (var part in parts)
                        {
                            if (part.StartsWith("63"))
                            {
                                if (long.TryParse(part, out long ticks))
                                {
                                    var date = new DateTime(ticks);

                                    if (date < DateTime.Now.AddDays(-7))
                                    {
                                        try
                                        {
                                            File.Delete(file);
                                        }
                                        catch
                                        { 
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
