using BookScraperWorker.Models;
using Microsoft.EntityFrameworkCore;

namespace BookScraperWorker.Data;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(ApplicationDbContext context, ILogger<BookRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Book?> GetByTituloAsync(string titulo)
    {
        return await _context.Livros.FirstOrDefaultAsync(b => b.Titulo == titulo);
    }

    public async Task<bool> AddAsync(Book book)
    {
        try
        {
            await _context.Livros.AddAsync(book);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar livro: {Titulo}", book.Titulo);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        try
        {
            _context.Livros.Update(book);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar livro: {Titulo}", book.Titulo);
            return false;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}