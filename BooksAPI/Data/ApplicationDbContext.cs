using BooksAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Livros { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuração da tabela e nomes das colunas
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("livros");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Titulo).HasColumnName("titulo");
            entity.Property(e => e.Preco).HasColumnName("preco");
            entity.Property(e => e.Estoque).HasColumnName("estoque");
            entity.Property(e => e.Avaliacao).HasColumnName("avaliacao");
            entity.Property(e => e.ImagemUrl).HasColumnName("imagem_url");
            entity.Property(e => e.Categoria).HasColumnName("categoria");
        });
    }
}