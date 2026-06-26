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
                    LogotipoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b4/ProtonMail_Logo.svg/120px-ProtonMail_Logo.svg.png",
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
                    LogotipoUrl = "https://upload.wikimedia.org/wikipedia/commons/6/60/Nextcloud_Logo.svg",
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
                    LogotipoUrl = "https://upload.wikimedia.org/wikipedia/commons/8/8d/Signal-Logo.svg",
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
                    LogotipoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/cc/Bitwarden_logo.svg",
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
                    LogotipoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c6/Moodle-logo.svg",
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
                    LogotipoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/48/Mastodon_Logotype_%28Simple%29.svg",
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
            };

            var nomesExistentes = context.ServicosDigitais.Select(s => s.Nome).ToHashSet();
            var novos = servicos.Where(s => !nomesExistentes.Contains(s.Nome)).ToList();
            if (novos.Count > 0)
            {
                context.ServicosDigitais.AddRange(novos);
                await context.SaveChangesAsync();
            }
        }
    }
}
