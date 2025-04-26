using System.Globalization;
using BookScraperWorker.Data;
using BookScraperWorker.Models;
using HtmlAgilityPack;

namespace BookScraperWorker.Services;

public class ScraperService : IScraperService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<ScraperService> _logger;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://books.toscrape.com/";

    public ScraperService(IBookRepository bookRepository, ILogger<ScraperService> logger, HttpClient httpClient)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task ScrapeAllBooksAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando processo de scraping...");
            
            // Obter as categorias primeiro
            var categories = await GetCategoriesAsync();
            
            foreach (var category in categories)
            {
                await ScrapeCategoryAsync(category.Key, category.Value);
            }
            
            _logger.LogInformation("Processo de scraping concluído com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o processo de scraping");
        }
    }

    private async Task<Dictionary<string, string>> GetCategoriesAsync()
    {
        var categoriesDict = new Dictionary<string, string>();
        
        try
        {
            var html = await _httpClient.GetStringAsync(BaseUrl);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var categoryNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='side_categories']//ul[@class='nav nav-list']//li//a");
            
            if (categoryNodes != null)
            {
                foreach (var node in categoryNodes.Skip(1)) // Pula o primeiro (que é 'Books')
                {
                    var categoryName = node.InnerText.Trim();
                    var categoryUrl = node.GetAttributeValue("href", "");
                    
                    if (!string.IsNullOrEmpty(categoryUrl))
                    {
                        categoriesDict.Add(categoryName, $"{BaseUrl}{categoryUrl}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias");
        }
        
        return categoriesDict;
    }

    private async Task ScrapeCategoryAsync(string categoryName, string categoryUrl)
    {
        _logger.LogInformation("Iniciando scraping da categoria: {Category}", categoryName);
        
        var currentUrl = categoryUrl;
        var hasNextPage = true;
        
        while (hasNextPage)
        {
            var html = await _httpClient.GetStringAsync(currentUrl);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            
            var bookNodes = htmlDocument.DocumentNode.SelectNodes("//article[@class='product_pod']");
            
            if (bookNodes != null)
            {
                foreach (var bookNode in bookNodes)
                {
                    var bookUrl = bookNode.SelectSingleNode(".//h3/a").GetAttributeValue("href", "");
                    
                    // Ajusta a URL do livro
                    if (!bookUrl.StartsWith("http"))
                    {
                        bookUrl = bookUrl.StartsWith("../") 
                            ? $"{BaseUrl}catalogue/{bookUrl.Replace("../", "")}" 
                            : $"{new Uri(new Uri(currentUrl), bookUrl).AbsoluteUri}";
                    }
                    
                    await ScrapeBookDetailsAsync(bookUrl, categoryName);
                }
            }
            
            // Verifica se há próxima página
            var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode("//li[@class='next']/a");
            
            if (nextPageNode != null)
            {
                var nextPageUrl = nextPageNode.GetAttributeValue("href", "");
                
                if (!string.IsNullOrEmpty(nextPageUrl))
                {
                    // Constrói a URL completa para a próxima página
                    currentUrl = currentUrl.Contains("/page-") 
                        ? currentUrl.Substring(0, currentUrl.LastIndexOf('/') + 1) + nextPageUrl
                        : currentUrl.TrimEnd('/') + "/" + nextPageUrl;
                }
                else
                {
                    hasNextPage = false;
                }
            }
            else
            {
                hasNextPage = false;
            }
        }
    }

    private async Task ScrapeBookDetailsAsync(string bookUrl, string categoryName)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(bookUrl);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            
            var title = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'product_main')]/h1").InnerText.Trim();
            
            // Verifica se o livro já existe
            var existingBook = await _bookRepository.GetByTituloAsync(title);
            var book = existingBook ?? new Book { Id = Guid.NewGuid(), Titulo = title, Categoria = categoryName };
            
            // Preço (remove o símbolo £)
            var priceStr = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='price_color']").InnerText.Trim();
            priceStr = priceStr.Replace("£", "").Trim();
            decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
            book.Preco = price;
            
            // Estoque
            var stockNode = htmlDocument.DocumentNode.SelectSingleNode("//p[contains(@class, 'availability')]");
            book.Estoque = stockNode != null ? stockNode.InnerText.Trim() : "Indisponível";
            
            // Avaliação (de 1 a 5)
            var ratingNode = htmlDocument.DocumentNode.SelectSingleNode("//p[contains(@class, 'star-rating')]");
            var ratingClass = ratingNode?.GetAttributeValue("class", "")?.Replace("star-rating", "")?.Trim();
            
            book.Avaliacao = ratingClass switch
            {
                "One" => 1,
                "Two" => 2,
                "Three" => 3,
                "Four" => 4,
                "Five" => 5,
                _ => 0
            };
            
            // URL da imagem
            var imageNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='item active']/img");
            var imageSrc = imageNode?.GetAttributeValue("src", "");
            
            if (!string.IsNullOrEmpty(imageSrc))
            {
                if (imageSrc.StartsWith("../"))
                {
                    book.ImagemUrl = $"{BaseUrl}{imageSrc.Replace("../", "")}";
                }
                else
                {
                    book.ImagemUrl = imageSrc.StartsWith("http") ? imageSrc : $"{BaseUrl}{imageSrc}";
                }
            }
            
            // Salva ou atualiza o livro
            if (existingBook == null)
            {
                await _bookRepository.AddAsync(book);
                _logger.LogInformation("Livro adicionado: {Title}", book.Titulo);
            }
            else
            {
                await _bookRepository.UpdateAsync(book);
                _logger.LogInformation("Livro atualizado: {Title}", book.Titulo);
            }
            
            await _bookRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair detalhes do livro: {Url}", bookUrl);
        }
    }
}