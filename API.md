# Documentação da API

Este documento descreve em detalhes a API REST implementada para o projeto de Coleta de Dados de Livros.

## Informações Gerais

- **Base URL**: `http://localhost:5015/api`
- **Formato de Resposta**: JSON
- **Autenticação**: Não implementada nesta versão

## Endpoints

### Listar Livros

Retorna uma lista paginada de livros, com opções de busca e filtro.

- **URL**: `/livros`
- **Método**: `GET`
- **Parâmetros de Query**:
  - `search` (opcional): Termo de busca para filtrar por título ou categoria
  - `pageNumber` (opcional): Número da página, começando em 1 (padrão: 1)
  - `pageSize` (opcional): Número de itens por página (padrão: 10, máximo: 50)

#### Exemplo de Requisição

```http
GET /api/livros?search=travel&pageNumber=1&pageSize=5
```

#### Exemplo de Resposta

```json
{
  "items": [
    {
      "id": "f8d9e31c-4e1d-4c1a-b6b3-3e9e4d6a1b5c",
      "titulo": "A Summer In Europe",
      "preco": 44.34,
      "estoque": "In stock (16 available)",
      "avaliacao": 4,
      "imagemUrl": "https://books.toscrape.com/media/cache/fe/1d/fe1d219f5f9639c1d346a1b648005a52.jpg",
      "categoria": "Travel"
    },
    {
      "id": "2b7c8f1d-5e2a-4d3b-8c9a-7f6e5d4c3b2a",
      "titulo": "A Year in Provence (Provence #1)",
      "preco": 56.88,
      "estoque": "In stock (30 available)",
      "avaliacao": 4,
      "imagemUrl": "https://books.toscrape.com/media/cache/db/80/db8032f20b2b157087fbd983e5a3536d.jpg",
      "categoria": "Travel"
    }
    // Mais itens...
  ],
  "pageNumber": 1,
  "pageSize": 5,
  "totalPages": 2,
  "totalCount": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Obter Detalhes de um Livro

Retorna os detalhes completos de um livro específico.

- **URL**: `/livros/{id}`
- **Método**: `GET`
- **Parâmetros de URL**:
  - `id`: ID do livro (UUID)

#### Exemplo de Requisição

```http
GET /api/livros/f8d9e31c-4e1d-4c1a-b6b3-3e9e4d6a1b5c
```

#### Exemplo de Resposta

```json
{
  "id": "f8d9e31c-4e1d-4c1a-b6b3-3e9e4d6a1b5c",
  "titulo": "A Summer In Europe",
  "preco": 44.34,
  "estoque": "In stock (16 available)",
  "avaliacao": 4,
  "imagemUrl": "https://books.toscrape.com/media/cache/fe/1d/fe1d219f5f9639c1d346a1b648005a52.jpg",
  "categoria": "Travel"
}
```

#### Resposta de Erro (Livro não encontrado)

```json
{
  "status": 404,
  "detail": "Livro com ID f8d9e31c-4e1d-4c1a-b6b3-3e9e4d6a1b5c não encontrado"
}
```

### Adicionar um Livro

Adiciona um novo livro ao banco de dados.

- **URL**: `/livros`
- **Método**: `POST`
- **Corpo da Requisição**:
  - `titulo` (obrigatório): Título do livro
  - `preco` (obrigatório): Preço do livro (decimal)
  - `estoque` (opcional): Status de estoque
  - `avaliacao` (opcional): Avaliação do livro (0-5)
  - `imagemUrl` (opcional): URL da imagem da capa
  - `categoria` (opcional): Categoria do livro

#### Exemplo de Requisição

```http
POST /api/livros
Content-Type: application/json

{
  "titulo": "O Guia do Mochileiro das Galáxias",
  "preco": 42.00,
  "estoque": "In stock (42 available)",
  "avaliacao": 5,
  "imagemUrl": "https://example.com/hitchhiker.jpg",
  "categoria": "Science Fiction"
}
```

#### Exemplo de Resposta (Sucesso)

```json
{
  "id": "3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r",
  "titulo": "O Guia do Mochileiro das Galáxias",
  "preco": 42.00,
  "estoque": "In stock (42 available)",
  "avaliacao": 5,
  "imagemUrl": "https://example.com/hitchhiker.jpg",
  "categoria": "Science Fiction"
}
```

### Atualizar um Livro

Atualiza os dados de um livro existente.

- **URL**: `/livros/{id}`
- **Método**: `PUT`
- **Parâmetros de URL**:
  - `id`: ID do livro (UUID)
- **Corpo da Requisição**: Mesmo formato do endpoint POST

#### Exemplo de Requisição

```http
PUT /api/livros/3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r
Content-Type: application/json

{
  "titulo": "O Guia do Mochileiro das Galáxias - Edição de Luxo",
  "preco": 52.00,
  "estoque": "In stock (10 available)",
  "avaliacao": 5,
  "imagemUrl": "https://example.com/hitchhiker-deluxe.jpg",
  "categoria": "Science Fiction"
}
```

#### Exemplo de Resposta (Sucesso)

```http
Status: 204 No Content
```

#### Resposta de Erro (Livro não encontrado)

```json
{
  "status": 404,
  "detail": "Livro com ID 3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r não encontrado"
}
```

### Excluir um Livro

Remove um livro do banco de dados.

- **URL**: `/livros/{id}`
- **Método**: `DELETE`
- **Parâmetros de URL**:
  - `id`: ID do livro (UUID)

#### Exemplo de Requisição

```http
DELETE /api/livros/3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r
```

#### Exemplo de Resposta (Sucesso)

```http
Status: 204 No Content
```

#### Resposta de Erro (Livro não encontrado)

```json
{
  "status": 404,
  "detail": "Livro com ID 3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r não encontrado"
}
```

## Códigos de Status

- `200 OK`: A requisição foi bem-sucedida
- `201 Created`: Um novo recurso foi criado com sucesso
- `204 No Content`: A requisição foi bem-sucedida, mas não há conteúdo para retornar
- `400 Bad Request`: A requisição está malformada ou contém parâmetros inválidos
- `404 Not Found`: O recurso solicitado não foi encontrado
- `500 Internal Server Error`: Ocorreu um erro interno no servidor

## Tratamento de Erros

A API retorna mensagens de erro em formato JSON com informações sobre o problema ocorrido.

Exemplo de resposta de erro:

```json
{
  "status": 400,
  "detail": "Erro interno ao processar a solicitação"
}
```

## Limitações e Considerações

- A paginação está limitada a 50 itens por página
- A busca é case-insensitive e pesquisa tanto no título quanto na categoria
- A API não implementa autenticação ou autorização
- Os IDs são gerados automaticamente como UUIDs
- Os preços são armazenados como valores decimais com duas casas

## Exemplo de Uso com cURL

### Listar Livros
```bash
curl -X GET "http://localhost:5015/api/livros?pageNumber=1&pageSize=10"
```

### Buscar por Termo
```bash
curl -X GET "http://localhost:5015/api/livros?search=mystery&pageNumber=1&pageSize=10"
```

### Obter Livro Específico
```bash
curl -X GET "http://localhost:5015/api/livros/3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r"
```

### Adicionar Livro
```bash
curl -X POST "http://localhost:5015/api/livros" \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Livro de Teste",
    "preco": 29.99,
    "estoque": "In stock",
    "avaliacao": 3,
    "imagemUrl": "https://example.com/test.jpg",
    "categoria": "Test"
  }'
```

### Atualizar Livro
```bash
curl -X PUT "http://localhost:5015/api/livros/3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r" \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Livro de Teste (Atualizado)",
    "preco": 39.99,
    "estoque": "In stock",
    "avaliacao": 4,
    "imagemUrl": "https://example.com/test-updated.jpg",
    "categoria": "Test"
  }'
```

### Excluir Livro
```bash
curl -X DELETE "http://localhost:5015/api/livros/3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r"
```