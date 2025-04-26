using BooksAPI.Data;
using BooksAPI.DTOs;
using BooksAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksAPI.Repositories;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(ApplicationDbContext context, ILogger<BookRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<Book>> GetAllAsync(string search, int pageNumber, int pageSize)
    {
        try
        {
            IQueryable<Book> query = _context.Livros;
            
            // Aplicar filtro de busca se fornecido
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(l => 
                    l.Titulo.ToLower().Contains(search) || 
                    l.Categoria.ToLower().Contains(search));
            }
            
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var livros = await query
                .OrderBy(l => l.Titulo) // Ordenação alfabética por título
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedResponse<Book>
            {
                Items = livros,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter lista paginada de livros");
            throw;
        }
    }

    // Sobrecarga para manter compatibilidade com o código existente
    public async Task<PaginatedResponse<Book>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await GetAllAsync("", pageNumber, pageSize);
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Livros.FindAsync(id);
    }

    public async Task<Book> AddAsync(Book book)
    {
        _context.Livros.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        _context.Entry(book).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExistsAsync(book.Id))
            {
                return false;
            }
            
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var book = await _context.Livros.FindAsync(id);
        
        if (book == null)
        {
            return false;
        }
        
        _context.Livros.Remove(book);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Livros.AnyAsync(e => e.Id == id);
    }
}