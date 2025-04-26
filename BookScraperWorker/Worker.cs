using BookScraperWorker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookScraperWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado em: {time}", DateTimeOffset.Now);

        var intervalInHours = _configuration.GetValue<int>("ScrapingIntervalInHours", 6);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Iniciando processo de scraping em: {time}", DateTimeOffset.Now);
            
            try
            {
                // Criar um escopo para os serviços
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Obter o serviço dentro do escopo
                    var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
                    await scraperService.ScrapeAllBooksAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processo de scraping");
            }
            
            _logger.LogInformation("Scraping concluído. Próxima execução em {hours} horas", intervalInHours);
            await Task.Delay(TimeSpan.FromHours(intervalInHours), stoppingToken);
        }
    }
}