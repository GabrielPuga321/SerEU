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

### Configuração do Email (SMTP)

Para que o envio de emails (confirmação de conta, recuperação de palavra-passe) funcione:

1. **Copia o ficheiro de exemplo:**
   ```bash
   cp SerEU/appsettings.example.json SerEU/appsettings.Development.json
   ```

2. **Preencha as credenciais** no ficheiro `appsettings.Development.json`:
   ```json
   {
     "EmailSettings": {
       "Host": "smtp.protonmail.ch",
       "Port": 587,
       "EnableSsl": true,
       "User": "info@sereu.diogop.eu",
       "Password": "ASQF4NQ1KDXQAURH",
       "FromEmail": "info@sereu.diogop.eu",
       "FromName": "SerEU"
     }
   }
   ```

3. **O ficheiro `.gitignore` já está configurado** para ignorar ficheiros de configuração com credenciais, pelo que o teu token não será exposto no repositório.

### Utilizadores de teste

| Papel | Email | Password |
|-------|-------|----------|
| Admin | admin@sereu.diogop.eu | Admin@1234 |
| Utilizador | utilizador@sereu.diogop.eu | User@1234 |

## API REST

`/api/servicos`, `/api/categorias`, `/api/tags`, `/api/avaliacoes`
