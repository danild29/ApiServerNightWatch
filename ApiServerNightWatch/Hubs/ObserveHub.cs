using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace ApiServerNightWatch.Hubs;

public interface IObserveHub: IChat, IErrable
{
    Task TeamJoined(string team);
}





public partial class ObserveHub : Hub<IObserveHub>
{
    private readonly ILogger<ObserveHub> _logger;
    private readonly EventManager _manager;
    private readonly IHubContext<GameHub, IGameHub> _gameHub;
    private const string _defaultGroupName = "Observers";

    public ObserveHub(ILogger<ObserveHub> logger, EventManager manager, IHubContext<GameHub, IGameHub> gameHub)
    {
        _logger = logger;
        _manager = manager;
        _gameHub = gameHub;
    }

    #region overrides

    [Authorize]
    public override async Task OnConnectedAsync()
    {
        var req = Context.GetHttpContext()!.Request;
        (string connectionId, int id) = (Context.ConnectionId, TokenManager.ParseToken(req));
        int? eventId = await _manager.ConnectManager(new ConnectionModel { Id = id, ConnectionId = connectionId });
        if (eventId == null) return;
        await base.OnConnectedAsync();


        await Groups.AddToGroupAsync(connectionId, _defaultGroupName);


        await RefreshMessages((int)eventId);

    }


    [Authorize]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var req = Context.GetHttpContext()!.Request;
        (string connectionId, int id) = (Context.ConnectionId, TokenManager.ParseToken(req));

        int? idUserRemoved = _manager.DisconnectManager(connectionId);
        if (idUserRemoved != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _defaultGroupName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion




    private async Task RefreshMessages(int id)
    {
        var data = await _manager.LoadMessages(id);

        foreach (MessageModel m in data)
        {
            var json = JsonSerializer.Serialize(m);
            await Clients.Caller.ReceiveMessage(json);
        }
    }

    public async Task<int> SendMessageToTeam(string me)
    {
        try
        {
            MessageModel? message = JsonSerializer.Deserialize<MessageModel>(me) ?? throw new ArgumentNullException(nameof(me));
            var e = await _manager.SaveMessage(message);

            var json = JsonSerializer.Serialize(message);

            foreach (var m in e.Managers)
            {
                await Clients.Client(m.ConnectionId).ReceiveMessage(json);
            }
            var team = e.Teams.FirstOrDefault(x => x.Id == message.FromTeam) ?? throw new ArgumentNullException(nameof(me));

            foreach (var t in team.Players)
            {
                await _gameHub.Clients.Client(t.ConnectionId).ReceiveMessage(json);
            }

            return message.Id;
        }
        catch (Exception)
        {

            throw;
        }
                
    }


    public async Task LoadAllParticipants(int eventId)
    {
        try
        {
            var res = _manager.FindEvent(eventId) ?? throw new ArgumentNullException(nameof(eventId));
            
            foreach(var t in res.Teams)
            {
                await Clients.Caller.TeamJoined(JsonSerializer.Serialize(t));
            }


        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            await Clients.Caller.ErrorOccured(JsonSerializer.Serialize(er));
        }

    }
}



