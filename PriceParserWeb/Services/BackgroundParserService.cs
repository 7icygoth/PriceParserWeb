using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PriceParserWeb.Services
{
    public class BackgroundParserService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundParserService> _logger;
        private Timer? _timer;

        public BackgroundParserService(IServiceProvider serviceProvider, ILogger<BackgroundParserService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await DoWork(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromHours(6));
            return Task.CompletedTask;
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Автоматический парсинг запущен...");

            using var scope = _serviceProvider.CreateScope();
            var parser = scope.ServiceProvider.GetRequiredService<IPriceParserService>();

            try
            {
                await parser.ParseAllProductsAsync();
                _logger.LogInformation("Парсинг завершён.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автоматическом парсинге");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}