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

## Pré-requisitos

### Configurar Utilizadores
Por defeito, o projeto cria dois utilizadores de teste ao iniciar. **Nunca uses estas credenciais em produção!**

#### Para desenvolvimento (Secret Manager)
O Secret Manager armazena credenciais localmente sem versionar no Git

##### Inicializar Secret Manager (na pasta SerEU)
cd SerEU
dotnet user-secrets init

###### Definir credenciais dos utilizadores seed
> Admin
```sh
dotnet user-secrets set "SeedUsers:Admin:Email" "admin@sereu.diogop.eu"
dotnet user-secrets set "SeedUsers:Admin:Password" "Admin@1234"
```

> Utilizador
```sh
dotnet user-secrets set "SeedUsers:User:Email" "utilizador@sereu.diogop.eu"
dotnet user-secrets set "SeedUsers:User:Password" "User@1234"
```

#### Ou ainda, no ficheiro de configurão
Pode-se também definir no `appsettings.json` 

```json
{
  "SeedUsers": {
    "Admin": {
      "Email": "",
      "Password": ""
    },
    "User": {
      "Email": "", 
      "Password": ""
    }
  }
}
```

#### Para produção
Em produção, define as variáveis de ambiente no servidor:

```bash
# Unix-like
export SeedUsers__Admin__Email="admin@teudominio.tld"
export SeedUsers__Admin__Password="UmaPassword"
export SeedUsers__User__Email="user@teudominio.tld"
export SeedUsers__User__Password="OutraPassword"

# Windows (PowerShell)
$env:SeedUsers__Admin__Email = "admin@teudominio.tld"
$env:SeedUsers__Admin__Password = "UmaPassword"
$env:SeedUsers__User__Email = "user@teudominio.tld"
$env:SeedUsers__User__Password = "OutraPassword"
```


### Configurar SMTP
Para que o envio de emails (confirmação de conta, recuperação de palavra-passe) funcione:

1. **Copia o ficheiro de exemplo:**
   ```bash
   cp SerEU/appsettings.example.json SerEU/appsettings.json
   ```

2. **Preencha as credenciais** no ficheiro com os seus dados SMTP (servidor, porta, utilizador, password, etc.).

## Como executar

```bash
cd SerEU
dotnet run
```


## API REST

`/api/servicos`, `/api/categorias`, `/api/tags`, `/api/avaliacoes`
