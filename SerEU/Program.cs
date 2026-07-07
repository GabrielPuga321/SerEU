using System.Threading.RateLimiting;
using SerEU.Data;
using SerEU.Hubs;
using SerEU.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Habilita Static Web Assets em desenvolvimento para servir CSS/JS gerados (CSS Scoping)
builder.WebHost.UseStaticWebAssets();

// Configuração da base de dados SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("String de ligação 'DefaultConnection' não encontrada.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuração do Identity com suporte a Roles e política de palavra-passe reforçada
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Palavra-passe forte: 8+ caracteres, dígitos, maiúsculas e símbolos
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // Bloqueio de conta após tentativas falhadas (proteção contra força bruta)
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        // Garante endereços de email únicos
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Endurecimento do cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;                       // Inacessível a JavaScript (mitiga XSS)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Apenas enviado por HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;           // Mitiga CSRF
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Serviço de envio de emails via SMTP (confirmação de conta, recuperação de palavra-passe)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, MailKitEmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Swagger / OpenAPI — documentação e teste interativo da API REST
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "SerEU API",
        Version = "v1",
        Description = "API REST da Lista Europeia de Serviços Digitais"
    });

    // Inclui os comentários /// dos controllers na documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Token antifalsificação (CSRF) enviado também via cabeçalho para chamadas à API
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// Configuração do SignalR para notificações em tempo real
builder.Services.AddSignalR();

// Limitação de pedidos (rate limiting) — protege contra abuso e ataques de negação de serviço
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Limite global por endereço IP para todos os pedidos
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 240,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Política mais restritiva para a API REST
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// Política de autorização para administradores
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApenasAdmin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Inicialização da base de dados com dados de seed
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InicializarAsync(scope.ServiceProvider);
}

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    // Swagger disponível apenas em desenvolvimento (não expor a API em produção)
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SerEU API v1");
        options.DocumentTitle = "SerEU API — Swagger";
    });
}
else
{
    app.UseExceptionHandler("/Erros/500");
    // HSTS — força HTTPS no navegador durante 1 ano
    app.UseHsts();
}

// Cabeçalhos de segurança HTTP aplicados a todas as respostas
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";          // Impede MIME-sniffing
    headers["X-Frame-Options"] = "DENY";                    // Impede clickjacking (embeber em iframe)
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["X-XSS-Protection"] = "0";                      // Desliga o filtro legado (CSP é a defesa moderna)
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    // Content-Security-Policy — restringe origens de scripts, estilos, imagens, etc.
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Aplica a limitação de pedidos depois do routing
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Páginas de erro personalizadas para códigos de estado HTTP
app.UseStatusCodePagesWithReExecute("/Erros/{0}");

app.MapStaticAssets();

// Rotas MVC (utilizadas pela API)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Razor Pages (interface principal da aplicação) — mapeado antes do MVC para ter prioridade em "/"
app.MapRazorPages()
    .WithStaticAssets();

// Hub SignalR
app.MapHub<NotificacoesHub>("/hubs/notificacoes");

app.Run();
