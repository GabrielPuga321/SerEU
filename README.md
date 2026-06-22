# European List of Digital Services (ELDSS)

Plataforma de procura e listagem de serviços digitais europeus ou abertos.
Trabalho prático de Desenvolvimento Web — Eng. Informática.

## Stack

- ASP.NET Core 10 (net10.0)
- Razor Pages (interface) + ASP.NET Core MVC (API REST)
- Entity Framework Core 10 + SQLite
- ASP.NET Core Identity (autenticação)
- SignalR (notificações em tempo real)
- Bootstrap 5 + Bootstrap Icons

## Organização do repositório

O trabalho está dividido em branches:

- **backend** — modelos, EF Core, API REST, serviços (email), SignalR, configuração
- **frontend** — layout, estilos e bibliotecas partilhadas
- **front-office** — área pública (serviços, autenticação, páginas de erro)
- **back-office** — área de administração (aprovações, categorias, tags)

A branch **main** centraliza todo o trabalho integrado.

## Como correr

```bash
cd EuropeanListofDigitalServices
dotnet run
```

### Utilizadores de teste

| Papel | Email | Password |
|-------|-------|----------|
| Admin | admin@eldss.eu | Admin@1234 |
| Utilizador | utilizador@eldss.eu | User@1234 |

## API REST

`/api/servicos`, `/api/categorias`, `/api/tags`, `/api/avaliacoes`
