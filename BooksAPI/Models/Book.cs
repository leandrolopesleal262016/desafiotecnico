using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksAPI.Models;

[Table("livros")]
public class Book
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Required]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required]
    [Column("preco", TypeName = "decimal(10, 2)")]
    public decimal Preco { get; set; }
    
    [Column("estoque")]
    public string Estoque { get; set; } = string.Empty;
    
    [Column("avaliacao")]
    public int Avaliacao { get; set; }
    
    [Column("imagem_url")]
    public string ImagemUrl { get; set; } = string.Empty;
    
    [Column("categoria")]
    public string Categoria { get; set; } = string.Empty;
}