using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo_close_stdout
{
    public sealed class DemoBackgroundService : BackgroundService
    {
        private readonly Task _backgroundTask;
        private readonly ILogger<DemoBackgroundService> _logger;
        public DemoBackgroundService(ILogger<DemoBackgroundService> logger)
        {
            _logger = logger;
            _backgroundTask = Task.Run(async () =>
            {
                while (true)
                {
                    _logger.LogInformation(DateTime.Now.ToString("yyyy-M-d dddd HH:mm:ss"));
                    await Task.Delay(1000);
                }
            });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}