using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ApiServerNightWatch.Hubs;

public interface IGameHub: IChat, IErrable
{
    Task EveryoneReceiveMessage(string message);
    Task ReceiveNextQuestion(string question);
    Task UpdateTeamates(string teammates);

    
}

public interface IChat
{
    Task ReceiveMessage(string message);
}
public interface IErrable
{
    Task ErrorOccured(string error);
}

public class GameHub : Hub<IGameHub>
{

    private readonly ILogger<GameHub> _logger;
    private readonly EventManager _manager;
    private readonly IQuestionData _question;
    private readonly IHubContext<ObserveHub, IObserveHub> _observeHub;
    private const string _defaultGroupName = "General";

    public GameHub(ILogger<GameHub> logger, EventManager manager, IQuestionData question,
        IHubContext<ObserveHub, IObserveHub> observeHub)
    {
        _logger = logger;
        _manager = manager;
        _question = question;
        _observeHub = observeHub;
    }

    #region overrides

    [Authorize]
    public override async Task OnConnectedAsync()
    {


        (string connectionId, int id) = (Context.ConnectionId, TokenManager.ParseToken(Context.GetHttpContext()!.Request));
        if (!await _manager.ConnectUser(new ConnectionModel { Id = id, ConnectionId = connectionId })) return;
        await base.OnConnectedAsync();
        await Groups.AddToGroupAsync(connectionId, _defaultGroupName);


        var team = await _manager.GetTeamByUserId(id);
        if (team is null) return;

        var questions = await _question.GetAllQuestionsFromEvent(team.EventId);
        await Clients.Client(connectionId).ReceiveNextQuestion(questions[team.CurQuestion].Question);


        await NotifyTeamates(id);
        await RefreshMessages(team.Id);

    }

    

    [Authorize]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        (string connectionId, int id) = (Context.ConnectionId, TokenManager.ParseToken(Context.GetHttpContext()!.Request));

        int? idUserRemoved = _manager.DisconnectUser(connectionId);
        if (idUserRemoved != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _defaultGroupName);
            await NotifyTeamates(id);
        }

        await base.OnDisconnectedAsync(exception);
    }


    #endregion



    #region functions

    private async Task RefreshMessages(int id)
    {
        var data = await _manager.LoadMessages(id);

        foreach (MessageModel m in data)
        {
            var json = JsonSerializer.Serialize(m);
            await Clients.Caller.ReceiveMessage(json);
        }
    }

    public async Task NotifyTeamates(int id)
    {

        var team = await _manager.GetTeamByUserId(id);
        if (team is null) return;


        string json = JsonSerializer.Serialize(team!.Players);

        //foreach (var p in team.Players)
        //{
        //    await Clients.Client(p.ConnectionId).UpdateTeamates(json);
        //}

        await Task.WhenAll(
            team.Players.Select(p => Clients.Client(p.ConnectionId).UpdateTeamates(json)));
    }
    #endregion





    [Authorize]
    public async Task AnswerQuestion(string answer)
    {

        int id = TokenManager.ParseToken(Context.GetHttpContext()!.Request);

        var team = await _manager.GetTeamByUserId(id);
        if (team is null) return;

        var questions = await _question.GetAllQuestionsFromEvent(team.EventId);

        OutputQuestion output = null!;
        if (questions[team.CurQuestion].Answer != answer)
        {
            output = new(answer, AnswerStatus.WrongAnswer, questions.Count, team.CurQuestion);

            var jsonData = JsonSerializer.Serialize(output);
            
            foreach (var p in team.Players)
                await Clients.Client(p.ConnectionId).ReceiveNextQuestion(jsonData);
            
            return;
        }

        team.CurQuestion++;
        output = new("", AnswerStatus.CompleteEvent, questions.Count, team.CurQuestion);

        if (output.IsLast == false)
        {
            output.Content = questions[team.CurQuestion].Question;
            output.Code = AnswerStatus.CorrectAnswer;
        }

        
        var json = JsonSerializer.Serialize(output);
        foreach (var p in team.Players)
            await Clients.Client(p.ConnectionId).ReceiveNextQuestion(json);
    }


    


    public async Task<int> SendMessageToObservers(string me)
    {
        try
        {
            MessageModel? message = JsonSerializer.Deserialize<MessageModel>(me);
            if (message is null) return 0;

            var e = await _manager.SaveMessage(message).ConfigureAwait(false);

            var json = JsonSerializer.Serialize(message);

            foreach (var m in e.Managers)
            {
                await _observeHub.Clients.Client(m.ConnectionId).ReceiveMessage(json);
            }

            var team = e.Teams.FirstOrDefault(x => x.Id == message.FromTeam) ?? throw new ArgumentNullException(nameof(me));

            foreach (var t in team.Players)
            {
                await Clients.Client(t.ConnectionId).ReceiveMessage(json);
            }

            return message.Id;
        }
        catch (Exception ex)
        {

            _logger.LogWarning(ex, null);
            return 0;
        }

    }


    public async Task SendMessageToAll(string message) => await Clients.Group(_defaultGroupName).EveryoneReceiveMessage(message);





}


public class OutputQuestion
{
    public int CurQuestion;
    public int TotalAmoutOfQuestion;
    public bool IsLast;

    public AnswerStatus Code;
    public string Content;
    public bool HasBinary;

    public OutputQuestion(string content, AnswerStatus code, int totalAmoutOfQuestion, int curQuestion = 0, bool hasContent = false)
    {
        TotalAmoutOfQuestion = totalAmoutOfQuestion;
        CurQuestion = curQuestion;
        Content = content;
        Code = code;
        HasBinary = hasContent;
        IsLast = totalAmoutOfQuestion == curQuestion;
    }

}


public enum AnswerStatus
{
    CorrectAnswer = 0,
    CompleteEvent = 1,
    WrongAnswer = 2,
}

