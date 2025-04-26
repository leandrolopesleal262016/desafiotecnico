using System.ComponentModel.DataAnnotations;

namespace BooksAPI.DTOs;

public class BookRequestDto
{
    [Required(ErrorMessage = "O título é obrigatório")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal Preco { get; set; }
    
    public string Estoque { get; set; } = string.Empty;
    
    [Range(0, 5, ErrorMessage = "A avaliação deve estar entre 0 e 5")]
    public int Avaliacao { get; set; }
    
    public string ImagemUrl { get; set; } = string.Empty;
    
    public string Categoria { get; set; } = string.Empty;
}