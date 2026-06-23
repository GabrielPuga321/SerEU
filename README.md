# SerEU
![logo](./assets/logo-sereu.svg)
> *Seja mais Europeu, e escolha serviços Europeus*

Plataforma de procura e listagem de serviços digitais europeus ou abertos.  
Trabalho prático de Desenvolvimento Web — Licenciatura Engenharia Informática.

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

## Como executar

```bash
cd SerEU
dotnet run
```

### Utilizadores de teste

| Papel | Email | Password |
|-------|-------|----------|
| Admin | admin@sereu.diogop.eu | Admin@1234 |
| Utilizador | utilizador@sereu.diogop.eu | User@1234 |

## API REST

`/api/servicos`, `/api/categorias`, `/api/tags`, `/api/avaliacoes`
