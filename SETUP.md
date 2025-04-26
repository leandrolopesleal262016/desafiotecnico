# Guia de Configuração

Este documento fornece instruções detalhadas para configurar e executar os diferentes componentes do projeto de Coleta de Dados de Livros.

## Sumário

- [Pré-requisitos](#pré-requisitos)
- [Configuração do Ambiente](#configuração-do-ambiente)
- [Configuração do Banco de Dados](#configuração-do-banco-de-dados)
- [Configuração do Worker Service](#configuração-do-worker-service)
- [Configuração da API](#configuração-da-api)
- [Configuração do Frontend](#configuração-do-frontend)
- [Execução com Docker](#execução-com-docker)
- [Verificação da Instalação](#verificação-da-instalação)

## Pré-requisitos

Antes de iniciar a configuração, certifique-se de ter instalado:

1. **.NET 8 SDK**
   - [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verifique a instalação: `dotnet --version` (deve mostrar 8.0.x)

2. **PostgreSQL 12+**
   - [Download PostgreSQL](https://www.postgresql.org/download/)
   - Lembre-se da senha do usuário 'postgres' configurada durante a instalação

3. **Visual Studio Code** (recomendado)
   - [Download Visual Studio Code](https://code.visualstudio.com/)
   - Extensões recomendadas:
     - C# Dev Kit
     - .NET Core Test Explorer
     - Live Server (para o frontend)

4. **Docker** (opcional)
   - [Download Docker Desktop](https://www.docker.com/products/docker-desktop)

## Configuração do Ambiente

### Estrutura de Diretórios

Crie a seguinte estrutura de diretórios para o projeto:

```
BooksScraperProject/
├── BookScraperWorker/
├── BooksAPI/
├── Frontend/
├── Database/
└── docker-compose.yml
```

## Configuração do Banco de Dados

### 1. Criar o Banco de Dados

#### Usando o SQL Shell (psql):

1. Abra o SQL Shell (instalado com o PostgreSQL)
2. Conecte-se com o usuário 'postgres' e informe a senha
3. Execute:

```sql
CREATE DATABASE bookscraper;
```

#### Usando o pgAdmin:

1. Abra o pgAdmin
2. Conecte-se ao servidor PostgreSQL
3. Clique com o botão direito em "Databases" > "Create" > "Database"
4. Nome: `bookscraper`, Owner: `postgres`
5. Clique em "Save"

### 2. Criar a Tabela

Crie um arquivo `Database/create_tables.sql` com o seguinte conteúdo:

```sql
CREATE TABLE IF NOT EXISTS livros (
    id UUID PRIMARY KEY,
    titulo TEXT NOT NULL,
    preco NUMERIC NOT NULL,
    estoque TEXT,
    avaliacao INT,
    imagem_url TEXT,
    categoria TEXT
);

CREATE INDEX IF NOT EXISTS idx_livros_titulo ON livros(titulo);
CREATE INDEX IF NOT EXISTS idx_livros_categoria ON livros(categoria);
```

Execute o script:

```bash
# No terminal
psql -U postgres -d bookscraper -f Database/create_tables.sql

# Ou use o pgAdmin:
# 1. Conecte-se ao banco de dados 'bookscraper'
# 2. Clique em "Query Tool"
# 3. Cole o conteúdo do script
# 4. Clique em "Execute"
```

## Configuração do Worker Service

### 1. Criar o Projeto Worker Service

```bash
cd BookScraperWorker
dotnet new worker
```

### 2. Adicionar Pacotes Necessários

```bash
dotnet add package HtmlAgilityPack
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.Extensions.Http
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### 3. Estrutura de Diretórios

Crie as seguintes pastas no projeto:
```
BookScraperWorker/
├── Models/
├── Data/
├── Services/
└── logs/  (será criada automaticamente)
```

### 4. Configuração

Edite o arquivo `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bookscraper;Username=postgres;Password=SuaSenhaAqui"
  },
  "ScrapingIntervalInHours": 6
}
```

### 5. Adicionar os Arquivos do Código

Adicione todos os arquivos de código para o Worker Service conforme disponibilizados no projeto.

## Configuração da API

### 1. Criar o Projeto API

```bash
cd ../BooksAPI
dotnet new webapi
```

### 2. Adicionar Pacotes Necessários

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Serilog.AspNetCore
```

### 3. Estrutura de Diretórios

Crie as seguintes pastas no projeto:
```
BooksAPI/
├── Controllers/
├── Models/
├── Data/
├── DTOs/
├── Repositories/
└── logs/  (será criada automaticamente)
```

### 4. Configuração

Edite o arquivo `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bookscraper;Username=postgres;Password=SuaSenhaAqui"
  }
}
```

### 5. Adicionar os Arquivos do Código

Adicione todos os arquivos de código para a API conforme disponibilizados no projeto.

## Configuração do Frontend

### 1. Estrutura de Diretórios

Crie a seguinte estrutura:
```
Frontend/
├── index.html
├── styles.css
└── js/
    └── app.js
```

### 2. Configuração

No arquivo `js/app.js`, configure a URL da API:

```javascript
// Variáveis globais
const API_URL = 'http://localhost:5015/api'; // Ajuste conforme necessário
```

## Execução com Docker

### 1. Criar o Docker Compose

Crie um arquivo `docker-compose.yml` na raiz do projeto com o conteúdo fornecido.

### 2. Criar Dockerfiles

#### Para a API (BooksAPI/Dockerfile):

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BooksAPI.csproj", "./"]
RUN dotnet restore "BooksAPI.csproj"
COPY . .
RUN dotnet build "BooksAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BooksAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BooksAPI.dll"]
```

#### Para o Worker Service (BookScraperWorker/Dockerfile):

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BookScraperWorker.csproj", "./"]
RUN dotnet restore "BookScraperWorker.csproj"
COPY . .
RUN dotnet build "BookScraperWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookScraperWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookScraperWorker.dll"]
```

### 3. Executar com Docker Compose

```bash
docker-compose up -d
```

## Verificação da Instalação

### 1. Verificar o Banco de Dados

```bash
psql -U postgres -d bookscraper -c "SELECT COUNT(*) FROM livros;"
```
Ou use o pgAdmin para executar a consulta.

### 2. Verificar a API

Abra um navegador e acesse:
```
http://localhost:5015/api/livros
```

### 3. Verificar o Worker Service

Verifique os logs gerados:
```bash
cat BookScraperWorker/logs/log-*.txt
```

### 4. Verificar o Frontend

Abra o arquivo `Frontend/index.html` em um navegador ou use a extensão Live Server do VSCode.

---

Após seguir estes passos, você deverá ter um ambiente de desenvolvimento completo e funcionando para o projeto de Coleta de Dados de Livros. Se encontrar problemas, consulte o arquivo README.md para resolução de problemas comuns.