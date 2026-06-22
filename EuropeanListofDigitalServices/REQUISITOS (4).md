# Requisitos — Trabalho Prático de Desenvolvimento Web
Licenciatura em Eng. Informática — 2º ano, 2º semestre

---

## Avaliação

| Componente | Descrição | Peso |
|---|---|---|
| Componente 1 | Interface para o utilizador (server-side) | 60% |
| Componente 2 | Especificação e programação de API | 30% |
| Componente 3 | Publicação da aplicação Web | 10% |

> ⚠️ Nota mínima de 7,0 valores nas Componentes 1 e 2 para aprovação.

---

## Tecnologias Obrigatórias

- **ASP.NET Core MVC** — para a API (Componente 2)
- **ASP.NET Core Razor Pages** — para a Aplicação Web (Componente 1)
- **Entity Framework** — acesso à base de dados
- **LINQ** — consultas à base de dados
- **SignalR** — comunicação em tempo real
- **Layouts de interfaces gráficas**

## Tecnologias Opcionais

- jQuery
- Bootstrap ou similar (Semantic, Fluent, Materialize, etc.)
- Outras bibliotecas públicas via NuGet ou web

---

## Base de Dados

- Mínimo de **3 tabelas**
- Pelo menos **1 relação muitos-para-um**
- Pelo menos **1 relação muitos-para-muitos**
- Configuração via ficheiro `appsettings.json`
- Se usar MySQL: fornecer scripts de criação da BD, tabelas, registos e utilizadores

---

## Funcionalidades Obrigatórias

- [ ] **CRUD completo** (Create, Read, Update, Delete) sobre as tabelas, incluindo os relacionamentos
- [ ] **Validação de dados** na inserção e edição
- [ ] **Validação de operações de remoção**
- [ ] **Controlo de acessos** — mínimo 2 tipos de utilizadores com permissões diferentes
- [ ] **Mensagens de erro** adequadas em toda a aplicação
- [ ] **Páginas de erro personalizadas** (404, erros de BD, etc.)
- [ ] **Regras de usabilidade** respeitadas na interface
- [ ] **Interface em Português** (obrigatório)

---

## Página "Sobre" / Créditos (obrigatória)

Acessível a partir da página inicial, deve conter:

- Nome do curso, disciplina e ano letivo
- Número e nome dos autores
- Bibliotecas, frameworks e código de terceiros utilizados (com origem)
- Credenciais dos utilizadores criados inicialmente (login e password)

---

## Código

- Obrigatoriamente **formatado e comentado**
- Código de terceiros é permitido desde que:
  - Seja justificada a sua utilização
  - Os autores saibam explicar o que faz
  - A origem esteja referenciada no código e na página de créditos
- ⚠️ A não referência de código de terceiros é considerada **plágio**

---

## Entrega

- [ ] Submetido no **e-learning**
- [ ] Código no **GitHub** (a qualidade dos commits é avaliada)
- [ ] Publicado num **servidor web**

---

## Defesa

- Duração: ~40 minutos
- Questões sobre o trabalho desenvolvido
- Possibilidade de desenvolver/alterar código durante a defesa
- Em ambiente à distância: usa a versão publicada no servidor web

---

## Notas Importantes

- Tema livre, mas **tem de ser aprovado pelos docentes**
- Grupos de **2 alunos**
- Componentes não implementadas devem ser **claramente identificadas** na aplicação
- Qualquer tentativa de fraude implica **reprovação automática**
