using BooksAPI.DTOs;
using BooksAPI.Models;
using BooksAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BooksAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<LivrosController> _logger;

    public LivrosController(IBookRepository bookRepository, ILogger<LivrosController> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    // GET: api/livros
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<Book>>> GetLivros(
        [FromQuery] string search = "",
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1 || pageSize > 50)
        {
            pageSize = 10;
        }
        
        try
        {
            var result = await _bookRepository.GetAllAsync(search, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter livros");
            return StatusCode(500, "Erro interno ao processar a solicitação");
        }
    }

    // GET: api/livros/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Book>> GetLivro(Guid id)
    {
        try
        {
            var livro = await _bookRepository.GetByIdAsync(id);

            if (livro == null)
            {
                return NotFound($"Livro com ID {id} não encontrado");
            }

            return Ok(livro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter livro com ID {Id}", id);
            return StatusCode(500, "Erro interno ao processar a solicitação");
        }
    }

    // POST: api/livros
    [HttpPost]
    public async Task<ActionResult<Book>> PostLivro(BookRequestDto bookDto)
    {
        try
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Titulo = bookDto.Titulo,
                Preco = bookDto.Preco,
                Estoque = bookDto.Estoque,
                Avaliacao = bookDto.Avaliacao,
                ImagemUrl = bookDto.ImagemUrl,
                Categoria = bookDto.Categoria
            };

            var result = await _bookRepository.AddAsync(book);
            
            return CreatedAtAction(nameof(GetLivro), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar livro");
            return StatusCode(500, "Erro interno ao processar a solicitação");
        }
    }

    // PUT: api/livros/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutLivro(Guid id, BookRequestDto bookDto)
    {
        try
        {
            if (!await _bookRepository.ExistsAsync(id))
            {
                return NotFound($"Livro com ID {id} não encontrado");
            }

            var existingBook = await _bookRepository.GetByIdAsync(id);
            
            if (existingBook == null)
            {
                return NotFound($"Livro com ID {id} não encontrado");
            }

            // Atualiza as propriedades do livro existente
            existingBook.Titulo = bookDto.Titulo;
            existingBook.Preco = bookDto.Preco;
            existingBook.Estoque = bookDto.Estoque;
            existingBook.Avaliacao = bookDto.Avaliacao;
            existingBook.ImagemUrl = bookDto.ImagemUrl;
            existingBook.Categoria = bookDto.Categoria;

            var success = await _bookRepository.UpdateAsync(existingBook);

            if (success)
            {
                return NoContent();
            }
            
            return StatusCode(500, "Erro ao atualizar o livro");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar livro com ID {Id}", id);
            return StatusCode(500, "Erro interno ao processar a solicitação");
        }
    }

    // DELETE: api/livros/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLivro(Guid id)
    {
        try
        {
            var success = await _bookRepository.DeleteAsync(id);

            if (!success)
            {
                return NotFound($"Livro com ID {id} não encontrado");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir livro com ID {Id}", id);
            return StatusCode(500, "Erro interno ao processar a solicitação");
        }
    }
}