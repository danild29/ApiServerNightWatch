

using System.Reflection;

namespace ApiServerNightWatch.Hubs.HubManagers;

public class EventManager
{
    
    public EventManager(IUserData user, ITeamData team, ILogger<EventManager> logger, IEventData eventData, IMessageData message)
    {
        _user = user;
        _event = eventData;
        _team = team;
        _logger = logger;
        _message = message;
    }

    private readonly IUserData _user;
    private readonly ITeamData _team;
    private readonly IEventData _event;
    private readonly IMessageData _message;
    private readonly ILogger<EventManager> _logger;

    public List<EventHubModel> Games = new();


    public async Task<int?> ConnectManager(ConnectionModel model)
    {
        var eve = await _event.GetEventInfoByCreator(model.Id);
        if(eve == null) return null;

        foreach(var g in Games)
        {
            if(g.Id == eve.Id)
            {
                g.Managers.Add(model);
                return g.Id;
            }
        }
        return null;

    }



    public int? DisconnectManager(string connectionId)
    {
        foreach (var g in Games)
        {
            foreach (var man in g.Managers)
            {
                if (man.ConnectionId == connectionId)
                {
                    return man.Id;
                }
            }
           
        }
        return null;
    }


    public async Task<bool> ConnectUser(ConnectionModel model)
    {

        User? curUser = await _user.GetUser(model.Id);
        if (curUser is null || curUser.TeamId is null) return false;

        Team? curTeam = await _team.GetTeam((int)curUser.TeamId);
        if (curTeam is null || curTeam.EventId is null) return false;

        EventModel? curEvent = await _event.GetEvent((int)curTeam.EventId);
        if (curEvent is null) return false;


        var e = Games.FirstOrDefault(x => x.Id == curEvent.Id);
        if (e is null)
        {
            var newTeam = new TeamHubModel { Id = curTeam.Id , EventId = curEvent.Id};
            newTeam.Players.Add(model);

            var newEvent = new EventHubModel { Id = curEvent.Id };
            newEvent.Teams.Add(newTeam);
            Games.Add(newEvent);
            return true;
        }

        foreach (var team in e.Teams)
        {
            if(team.Id == curTeam.Id)
            {
                team.Players.Add(model);
                return true;
            }
        }

        var newTeamModel = new TeamHubModel { Id = curTeam.Id, EventId = curEvent.Id };
        newTeamModel.Players.Add(model);
        e.Teams.Add(newTeamModel);

        return true;
    }


    public int? DisconnectUser(string connectionId)
    {
        int? id = null;

        foreach(var g in Games) {
            foreach(var t in g.Teams) {
                foreach (var p in t.Players) 
                {
                    if(p.ConnectionId == connectionId)
                    {
                        id  = p.Id;
                        t.Players.Remove(p);
                        if(t.Players.Count == 0)
                        {
                            g.Teams.Remove(t);
                            if(g.Teams.Count == 0)
                            {
                                Games.Remove(g);
                            }
                        }
                        return id;
                    }
                }
            }
        }
        return null;
    }



    #region search

    public async Task<TeamHubModel?> GetTeam(int teamId)
    {
        foreach (var game in Games)
        {
            foreach (var team in game.Teams)
            {
                if (team.Id == teamId) return team;
            }
        }
        return null;
    }


    public async Task<TeamHubModel?> GetTeamByUserId(int userId)
    {
        var user = await _user.GetUser(userId);
        if (user is null || user.TeamId is null) return null;

        return await GetTeam((int)user.TeamId);
    }

    public List<string> FindAllConnectionsByUserId(int to)
    {
        List<string> cons = new List<string>();

        foreach (var g in Games)
        {
            foreach (var t in g.Teams)
            {
                foreach (var p in t.Players)
                {
                    if (p.Id == to)
                    {
                        cons.Add(p.ConnectionId);

                    }
                }
            }
        }
        return cons;
    }

    public List<string> FindAllConnectionsbyEventId(int to)
    {
        List<string> cons = new List<string>();

        foreach (var g in Games)
        {
            if(g.Id == to) return cons;
        }
        return cons;
    }



    public EventHubModel? FindEvent(int eventId)
    {
        foreach (var g in Games)
        {
            if (g.Id == eventId)
                return g;
        }
        return null;
    }

    #endregion


    public async Task<IEnumerable<MessageModel>> LoadMessages(int id)
    {
        List<MessageModel> messages = new List<MessageModel>();
        var res = await _message.GetCompanions(id);

        foreach (int companion in res)
        {
            var mes = await _message.GetNewMessages(id, companion);
            messages.AddRange(mes);
        }
        return messages;
    }
    public async Task<EventHubModel> SaveMessage(MessageModel message)
    {
        int newId = await _message.SaveMesssage(message);
        message.Id = newId;
        var e = FindEvent(message.ToEvent);
        return e;
    }
}


public class EventHubModel
{
    public required int Id;
    public string Mode = string.Empty;
    public List<ConnectionModel> Managers = new();
    public List<TeamHubModel> Teams = new();
}

public class TeamHubModel
{
    public required int Id;
    public required int EventId;
    public int CurQuestion = 0;
    public List<ConnectionModel> Players = new();
}

public class ConnectionModel
{
    public required int Id { get; set; }
    public required string ConnectionId { get; set; }

}


