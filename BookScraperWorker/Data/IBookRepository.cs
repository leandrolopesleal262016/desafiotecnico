using BookScraperWorker.Models;

namespace BookScraperWorker.Data;

public interface IBookRepository
{
    Task<Book?> GetByTituloAsync(string titulo);
    Task<bool> AddAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task SaveChangesAsync();
}