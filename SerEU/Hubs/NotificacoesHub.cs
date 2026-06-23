using Microsoft.AspNetCore.SignalR;

namespace SerEU.Hubs;

/// <summary>
/// Hub SignalR para envio de notificações em tempo real aos utilizadores.
/// Utilizado para notificar quando novos serviços são aprovados e para
/// avisar os administradores de que existem submissões pendentes.
/// </summary>
public class NotificacoesHub : Hub
{
    /// <summary>
    /// Nome do grupo que reúne todos os administradores ligados.
    /// As notificações de aprovações pendentes são enviadas apenas a este grupo.
    /// </summary>
    public const string GrupoAdministradores = "Administradores";

    /// <summary>
    /// Ao ligar, os administradores são adicionados a um grupo próprio para
    /// receberem as notificações de aprovações pendentes.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoAdministradores);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Envia uma notificação a todos os clientes ligados.
    /// </summary>
    public async Task EnviarNotificacao(string mensagem)
    {
        await Clients.All.SendAsync("ReceberNotificacao", mensagem);
    }

    /// <summary>
    /// Notifica todos os clientes que um novo serviço foi aprovado.
    /// </summary>
    public async Task ServicoAprovado(string nomeServico)
    {
        await Clients.All.SendAsync("NovoServicoAprovado", nomeServico);
    }
}