using BooksAPI.DTOs;
using BooksAPI.Models;

namespace BooksAPI.Repositories;

public interface IBookRepository
{
    Task<PaginatedResponse<Book>> GetAllAsync(string search, int pageNumber, int pageSize);
    Task<PaginatedResponse<Book>> GetAllAsync(int pageNumber, int pageSize);
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book> AddAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}