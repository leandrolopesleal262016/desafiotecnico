namespace BookScraperWorker.Services;

public interface IScraperService
{
    Task ScrapeAllBooksAsync();
}