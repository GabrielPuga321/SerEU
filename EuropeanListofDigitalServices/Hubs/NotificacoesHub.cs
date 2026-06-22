using Microsoft.AspNetCore.SignalR;

namespace EuropeanListofDigitalServices.Hubs;

/// <summary>
/// Hub SignalR para envio de notificações em tempo real aos utilizadores.
/// Utilizado para notificar quando novos serviços são aprovados.
/// </summary>
public class NotificacoesHub : Hub
{
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
