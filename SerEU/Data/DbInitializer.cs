using SerEU.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SerEU.Data;

/// <summary>
/// Inicializa a base de dados com dados de seed (roles, utilizadores e dados iniciais).
/// </summary>
public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Aplica migrações pendentes automaticamente (cria BD se não existir)
        await context.Database.MigrateAsync();

        // --- Criar Roles ---
        string[] roles = ["Admin", "Utilizador"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // --- Criar utilizador Admin ---
        var adminEmail = configuration["SeedUsers:Admin:Email"];
        var adminPassword = configuration["SeedUsers:Admin:Password"];
        
        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // --- Criar utilizador comum ---
        var userEmail = configuration["SeedUsers:User:Email"];
        var userPassword = configuration["SeedUsers:User:Password"];
        
        if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(userPassword))
        {
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new IdentityUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, userPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Utilizador");
            }
        }

        // --- Categorias iniciais ---
        if (!context.Categorias.Any())
        {
            var categorias = new List<Categoria>
            {
                new() { Nome = "Comunicação", Descricao = "Serviços de mensagens, email e videoconferência.", Icone = "bi-chat-dots" },
                new() { Nome = "Produtividade", Descricao = "Ferramentas de escritório, gestão de tarefas e notas.", Icone = "bi-briefcase" },
                new() { Nome = "Segurança & Privacidade", Descricao = "VPNs, gestores de passwords e serviços de encriptação.", Icone = "bi-shield-lock" },
                new() { Nome = "Armazenamento", Descricao = "Serviços de armazenamento e partilha de ficheiros na nuvem.", Icone = "bi-cloud" },
                new() { Nome = "Colaboração", Descricao = "Plataformas para trabalho em equipa e gestão de projetos.", Icone = "bi-people" },
                new() { Nome = "Educação", Descricao = "Plataformas de aprendizagem, cursos e recursos educativos.", Icone = "bi-book" },
            };
            context.Categorias.AddRange(categorias);
            await context.SaveChangesAsync();
        }

        // --- Tags iniciais ---
        if (!context.Tags.Any())
        {
            var tags = new List<Tag>
            {
                new() { Nome = "Open Source" },
                new() { Nome = "Europeu" },
                new() { Nome = "Gratuito" },
                new() { Nome = "FOSS" },
                new() { Nome = "Auto-hospedado" },
                new() { Nome = "Móvel" },
                new() { Nome = "Web" },
                new() { Nome = "GDPR" },
                new() { Nome = "Sem publicidade" },
                new() { Nome = "Descentralizado" },
            };
            context.Tags.AddRange(tags);
            await context.SaveChangesAsync();
        }

        // --- Serviços iniciais ---
        // Seed idempotente: adiciona apenas os serviços que ainda não existem (por nome),
        // permitindo introduzir novos serviços mesmo numa base de dados já existente.
        {
            var adminUser = !string.IsNullOrEmpty(adminEmail) 
                ? await userManager.FindByEmailAsync(adminEmail)
                : null;
            var categorias = context.Categorias.ToList();
            var tags = context.Tags.ToList();

            var servicos = new List<ServicoDigital>
            {
                new()
                {
                    Nome = "Proton Mail",
                    Descricao = "Serviço de email seguro e encriptado com sede na Suíça. Protege as comunicações com encriptação de ponta-a-ponta e não recolhe dados dos utilizadores.",
                    Url = "https://proton.me/mail",
                    Pais = "Suíça",
                    Licenca = "Freemium",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-30),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Sem publicidade" or "Web").ToList()
                },
                new()
                {
                    Nome = "Nextcloud",
                    Descricao = "Plataforma open source de armazenamento e colaboração na nuvem. Permite gerir ficheiros, calendários, contactos e muito mais de forma autónoma.",
                    Url = "https://nextcloud.com",
                    Pais = "Alemanha",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Armazenamento").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-25),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Auto-hospedado" or "GDPR").ToList()
                },
                new()
                {
                    Nome = "Signal",
                    Descricao = "Aplicação de mensagens instantâneas com encriptação de ponta-a-ponta. Desenvolvida por uma organização sem fins lucrativos com foco total na privacidade.",
                    Url = "https://signal.org",
                    Pais = "Estados Unidos",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-20),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Gratuito" or "Móvel").ToList()
                },
                new()
                {
                    Nome = "Bitwarden",
                    Descricao = "Gestor de passwords open source e gratuito. Sincroniza as credenciais em todos os dispositivos com encriptação de ponta-a-ponta.",
                    Url = "https://bitwarden.com",
                    Pais = "Estados Unidos",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-15),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Gratuito" or "Web" or "Móvel").ToList()
                },
                new()
                {
                    Nome = "Moodle",
                    Descricao = "Plataforma de aprendizagem online open source amplamente utilizada em universidades e escolas europeias. Suporta cursos, fóruns, testes e muito mais.",
                    Url = "https://moodle.org",
                    Pais = "Austrália",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Educação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-10),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Auto-hospedado" or "Gratuito" or "Web").ToList()
                },
                new()
                {
                    Nome = "Mastodon",
                    Descricao = "Rede social descentralizada e open source baseada no protocolo ActivityPub. Permite comunicação sem algoritmos publicitários, distribuída por milhares de servidores independentes.",
                    Url = "https://joinmastodon.org",
                    Pais = "Alemanha",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-9),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Descentralizado" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "LibreOffice",
                    Descricao = "Suite de produtividade open source e gratuita: processador de texto, folha de cálculo, apresentações e mais. Mantida pela The Document Foundation, sediada na Alemanha.",
                    Url = "https://www.libreoffice.org",
                    Pais = "Alemanha",
                    Licenca = "MPL-2.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Produtividade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-8),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Gratuito" or "Europeu").ToList()
                },
                new()
                {
                    Nome = "Jitsi Meet",
                    Descricao = "Plataforma de videoconferência open source e gratuita, sem necessidade de criar conta. Permite reuniões encriptadas diretamente no navegador.",
                    Url = "https://meet.jit.si",
                    Pais = "França",
                    Licenca = "Apache-2.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-7),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Gratuito" or "Web" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "Mullvad VPN",
                    Descricao = "Serviço de VPN sueco focado na privacidade. Não exige email para registo, aceita pagamentos anónimos e não mantém registos de atividade dos utilizadores.",
                    Url = "https://mullvad.net",
                    Pais = "Suécia",
                    Licenca = "Proprietário (cliente open source)",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-6),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "CryptPad",
                    Descricao = "Suite colaborativa encriptada de ponta-a-ponta: documentos, folhas de cálculo e quadros partilhados sem que o servidor consiga ler os conteúdos. Desenvolvida em França.",
                    Url = "https://cryptpad.org",
                    Pais = "França",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Colaboração").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-5),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Auto-hospedado" or "GDPR" or "Web").ToList()
                },
                new()
                {
                    Nome = "KeePassXC",
                    Descricao = "Gestor de palavras-passe open source, gratuito e totalmente local. Guarda as credenciais numa base de dados encriptada no próprio dispositivo, sem nuvem.",
                    Url = "https://keepassxc.org",
                    Pais = "Alemanha",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-4),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Gratuito").ToList()
                },
                new()
                {
                    Nome = "ONLYOFFICE",
                    Descricao = "Suite de escritório online open source com edição colaborativa de documentos compatível com formatos da Microsoft. Pode ser auto-hospedada. Sediada na Letónia.",
                    Url = "https://www.onlyoffice.com",
                    Pais = "Letónia",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Produtividade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddDays(-3),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Auto-hospedado" or "Web").ToList()
                },
                new()
                {
                    Nome = "Tuta (Tutanota)",
                    Descricao = "Serviço de email encriptado de ponta-a-ponta sediado na Alemanha. Encripta também o calendário e os contactos, sem publicidade nem rastreio.",
                    Url = "https://tuta.com",
                    Pais = "Alemanha",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-60),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "GDPR" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "Element",
                    Descricao = "Cliente de mensagens seguro construído sobre o protocolo aberto Matrix. Permite comunicação descentralizada e encriptada entre servidores independentes.",
                    Url = "https://element.io",
                    Pais = "Reino Unido",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-57),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Descentralizado" or "Web").ToList()
                },
                new()
                {
                    Nome = "Threema",
                    Descricao = "Aplicação de mensagens suíça com encriptação de ponta-a-ponta que funciona sem número de telefone nem email. Foco máximo em anonimato e minimização de dados.",
                    Url = "https://threema.ch",
                    Pais = "Suíça",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-54),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Móvel" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "Thunderbird",
                    Descricao = "Cliente de email open source da Mozilla, com suporte para várias contas, calendário e extensões. Gratuito e respeitador da privacidade.",
                    Url = "https://www.thunderbird.net",
                    Pais = "Estados Unidos",
                    Licenca = "MPL-2.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Comunicação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-51),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Gratuito").ToList()
                },
                new()
                {
                    Nome = "Joplin",
                    Descricao = "Aplicação open source de notas e tarefas com encriptação de ponta-a-ponta e sincronização à escolha do utilizador. Suporta Markdown e funciona offline.",
                    Url = "https://joplinapp.org",
                    Pais = "França",
                    Licenca = "MIT",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Produtividade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-48),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Gratuito" or "Móvel").ToList()
                },
                new()
                {
                    Nome = "Standard Notes",
                    Descricao = "Aplicação de notas encriptadas de ponta-a-ponta, simples e focada na longevidade dos dados. Multiplataforma e com sincronização segura.",
                    Url = "https://standardnotes.com",
                    Pais = "Estados Unidos",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Produtividade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-45),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Gratuito" or "Web").ToList()
                },
                new()
                {
                    Nome = "Vivaldi",
                    Descricao = "Navegador web altamente personalizável criado na Noruega, com foco na privacidade e em funcionalidades de produtividade integradas, sem recolha de dados.",
                    Url = "https://vivaldi.com",
                    Pais = "Noruega",
                    Licenca = "Proprietário",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Produtividade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-42),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "Gratuito" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "GIMP",
                    Descricao = "Editor de imagem open source e gratuito, alternativa completa a suites comerciais de edição gráfica. Mantido por uma comunidade internacional de voluntários.",
                    Url = "https://www.gimp.org",
                    Pais = "Internacional",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Produtividade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-39),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Gratuito").ToList()
                },
                new()
                {
                    Nome = "Proton VPN",
                    Descricao = "Serviço de VPN suíço focado na privacidade, com cliente open source e política de não registo. Permite navegar de forma segura e contornar a censura.",
                    Url = "https://protonvpn.com",
                    Pais = "Suíça",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-36),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "GDPR" or "Móvel").ToList()
                },
                new()
                {
                    Nome = "Startpage",
                    Descricao = "Motor de busca dos Países Baixos que entrega resultados sem perfilar os utilizadores nem guardar o histórico de pesquisas. Conforme com o RGPD.",
                    Url = "https://www.startpage.com",
                    Pais = "Países Baixos",
                    Licenca = "Proprietário",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-33),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Sem publicidade" or "Web").ToList()
                },
                new()
                {
                    Nome = "Qwant",
                    Descricao = "Motor de busca francês que não rastreia os utilizadores nem recolhe dados pessoais para publicidade direcionada. Alternativa europeia respeitadora da privacidade.",
                    Url = "https://www.qwant.com",
                    Pais = "França",
                    Licenca = "Proprietário",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-30),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Sem publicidade" or "Web").ToList()
                },
                new()
                {
                    Nome = "Vaultwarden",
                    Descricao = "Implementação leve e open source compatível com o Bitwarden, ideal para auto-hospedagem de um gestor de palavras-passe num servidor próprio.",
                    Url = "https://github.com/dani-garcia/vaultwarden",
                    Pais = "Internacional",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Segurança & Privacidade").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-27),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "FOSS" or "Auto-hospedado" or "Gratuito").ToList()
                },
                new()
                {
                    Nome = "pCloud",
                    Descricao = "Serviço suíço de armazenamento na nuvem com encriptação opcional do lado do cliente e planos vitalícios. Sincroniza ficheiros entre todos os dispositivos.",
                    Url = "https://www.pcloud.com",
                    Pais = "Suíça",
                    Licenca = "Proprietário",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Armazenamento").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-24),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Móvel").ToList()
                },
                new()
                {
                    Nome = "Tresorit",
                    Descricao = "Armazenamento na nuvem com encriptação de ponta-a-ponta de origem suíça/húngara, orientado para empresas que precisam de partilha segura de ficheiros.",
                    Url = "https://tresorit.com",
                    Pais = "Suíça",
                    Licenca = "Proprietário",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Armazenamento").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-21),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Europeu" or "GDPR" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "Filen",
                    Descricao = "Serviço alemão de armazenamento na nuvem com encriptação de ponta-a-ponta de conhecimento zero. Cliente open source e conformidade com o RGPD.",
                    Url = "https://filen.io",
                    Pais = "Alemanha",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Armazenamento").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-18),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "GDPR").ToList()
                },
                new()
                {
                    Nome = "OpenProject",
                    Descricao = "Plataforma open source alemã de gestão de projetos com planeamento, quadros ágeis e acompanhamento de tarefas. Pode ser auto-hospedada.",
                    Url = "https://www.openproject.org",
                    Pais = "Alemanha",
                    Licenca = "GPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Colaboração").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-15),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Auto-hospedado" or "Web").ToList()
                },
                new()
                {
                    Nome = "Taiga",
                    Descricao = "Ferramenta open source espanhola de gestão ágil de projetos, com suporte para Scrum e Kanban. Pensada para equipas que valorizam software livre.",
                    Url = "https://www.taiga.io",
                    Pais = "Espanha",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Colaboração").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-12),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Auto-hospedado" or "Web").ToList()
                },
                new()
                {
                    Nome = "Codeberg",
                    Descricao = "Plataforma de alojamento de código baseada em Forgejo, gerida por uma associação alemã sem fins lucrativos. Alternativa europeia e open source ao GitHub.",
                    Url = "https://codeberg.org",
                    Pais = "Alemanha",
                    Licenca = "MIT",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Colaboração").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-9),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Gratuito" or "Web").ToList()
                },
                new()
                {
                    Nome = "PeerTube",
                    Descricao = "Plataforma descentralizada e open source de partilha de vídeo desenvolvida em França pela Framasoft. Funciona em federação, sem algoritmos publicitários.",
                    Url = "https://joinpeertube.org",
                    Pais = "França",
                    Licenca = "AGPL-3.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Colaboração").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-6),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Europeu" or "Descentralizado" or "Sem publicidade").ToList()
                },
                new()
                {
                    Nome = "Sandstorm",
                    Descricao = "Plataforma open source para auto-hospedar aplicações web de forma simples e segura, com isolamento de cada aplicação numa caixa de areia.",
                    Url = "https://sandstorm.io",
                    Pais = "Internacional",
                    Licenca = "Apache-2.0",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Colaboração").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-3),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Open Source" or "Auto-hospedado" or "Web").ToList()
                },
                new()
                {
                    Nome = "OpenLearn",
                    Descricao = "Plataforma gratuita de recursos educativos abertos da The Open University (Reino Unido), com milhares de cursos e materiais de aprendizagem livres.",
                    Url = "https://www.open.edu/openlearn/",
                    Pais = "Reino Unido",
                    Licenca = "Creative Commons",
                    LogotipoUrl = null,
                    CategoriaId = categorias.First(c => c.Nome == "Educação").Id,
                    Aprovado = true,
                    DataSubmissao = DateTime.UtcNow.AddHours(-1),
                    UtilizadorId = adminUser?.Id,
                    Tags = tags.Where(t => t.Nome is "Gratuito" or "Web" or "Sem publicidade").ToList()
                },
            };

            var nomesExistentes = context.ServicosDigitais.Select(s => s.Nome).ToHashSet();
            var novos = servicos.Where(s => !nomesExistentes.Contains(s.Nome)).ToList();
            if (novos.Count > 0)
            {
                context.ServicosDigitais.AddRange(novos);
                await context.SaveChangesAsync();
            }
        }

        // --- Logótipos em falta ou inválidos ---
        // Para qualquer serviço sem logótipo (seed ou submetido por utilizadores),
        // gera a imagem respetiva a partir do domínio do próprio serviço.
        // Também corrige bases de dados antigas que guardaram URLs da Wikimedia
        // (alguns desses ficheiros deixaram de resolver, ex.: Proton Mail).
        {
            var semLogo = context.ServicosDigitais
                .Where(s => s.LogotipoUrl == null
                            || s.LogotipoUrl == ""
                            || s.LogotipoUrl.Contains("wikimedia.org"))
                .ToList();

            foreach (var servico in semLogo)
            {
                var url = ObterLogotipoPorDominio(servico.Url);
                if (url != null)
                    servico.LogotipoUrl = url;
            }

            if (semLogo.Count > 0)
                await context.SaveChangesAsync();
        }
    }

    // Constrói o URL do logótipo de um serviço a partir do domínio do seu site,
    // usando o serviço de favicons da Google (devolve a imagem de marca respetiva).
    private static string? ObterLogotipoPorDominio(string? siteUrl)
    {
        if (string.IsNullOrWhiteSpace(siteUrl)) return null;
        if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var uri)) return null;

        return $"https://www.google.com/s2/favicons?domain={uri.Host}&sz=128";
    }
}
