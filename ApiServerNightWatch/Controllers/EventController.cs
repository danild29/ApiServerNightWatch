namespace ApiServerNightWatch.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;
    private readonly ITeamData _teamData;
    private readonly IUserData _userData;
    private readonly IEventData _data;

    public EventController(ILogger<EventController> logger, ITeamData teamData, IUserData userData, IEventData data)
    {
        _logger = logger;
        _teamData = teamData;
        _userData = userData;
        _data = data;
    }

    [HttpPost, Route("create")]
    public async Task<IResult> Create(EventModel req)
    {
        try
        {
            {
                //var result = _validator.Validate(req);

                //if (result.IsValid == false)
                //{
                //    var error = new ErrorModel("Не удалось зарегистрировать данного пользователя");
                //    foreach (FluentValidation.Results.ValidationFailure er in result.Errors)
                //    {
                //        error.Errors.Add(er.ErrorMessage);
                //    }

                //    return Results.BadRequest(error); ;
                //}
            }

            req.StartAt = req.StartAt.ToUniversalTime();

            EventModel? eve = await _data.CreateEvent(req);
            if (eve == null)
            {
                return Results.BadRequest(new ErrorModel("Не удалось зарегистрировать данное мероприятие"));
            }

            return Results.Ok(eve);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }

    [HttpPost, Route("start")]
    public async Task<IResult> StartGame(int teamId)
    {
        try
        {
            Team? team = await _teamData.GetTeam(teamId);
            if(team is null) return Results.BadRequest(new ErrorModel("такой команды нету"));
            if(team.EventId is null) return Results.BadRequest(new ErrorModel("команда не зарегистрирована на событие"));

            EventModel eve = await _data.GetEvent((int)team.EventId) ?? throw new Exception("event shouldn't be null. wtf");

            if(eve.StartAt >= DateTime.UtcNow) return Results.Ok();

            return Results.BadRequest(new ErrorModel($"мероприятие начанется в {eve.StartAt}. Сейчас: {DateTime.UtcNow}"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }


    [HttpPost, Route("get")]
    public async Task<IResult> GetEventWithAllInfo(int id)
    {
        try
        {
            var res = await _data.GetEvent(id).ConfigureAwait(false) ?? throw new Exception("такого мероприятия не существует");
            res.Teams = (await _teamData.GetTeamsByEventId(res.Id).ConfigureAwait(false)).ToList();


            foreach(var team in res.Teams)
            {
                team.Players = (await _userData.GetAllTeamates(team.Id)).ToList();
            }

            return Results.Ok(res);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }






}