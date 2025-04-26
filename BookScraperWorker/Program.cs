using BookScraperWorker;
using BookScraperWorker.Data;
using BookScraperWorker.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Configurando o Host
var builder = Host.CreateDefaultBuilder(args);

// Configurando o Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Construindo o host com as configurações
builder.UseSerilog();
builder.ConfigureServices((hostContext, services) =>
{
    var configuration = hostContext.Configuration;
    
    // Configurando o contexto do banco de dados
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    
    // Registrando serviços
    services.AddScoped<IBookRepository, BookRepository>();
    services.AddScoped<IScraperService, ScraperService>();
    
    // Adicionando HttpClient
    services.AddHttpClient();
    
    // Registrando o Worker
    services.AddHostedService<Worker>();
});

// Construindo e executando o host
var host = builder.Build();
await host.RunAsync();