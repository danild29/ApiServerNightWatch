namespace ApiServerNightWatch.Controllers;

[ApiController]
[Route("api/[controller]")]

public sealed class QuestionController : ControllerBase
{
    private readonly ILogger<QuestionController> _logger;
    private readonly IQuestionData _data;

    public QuestionController(ILogger<QuestionController> logger, IQuestionData data)
    {
        _logger = logger;
        _data = data;
    }

    [HttpPost, Route("create")]
    public async Task<IResult> Create(QuestionModel que)
    {
        try
        {
            QuestionModel? res = await _data.CreateQuestion(que);
            return Results.Ok(res);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }

    }



    [HttpGet, Route("getEventQuestions")]
    public async Task<IResult> GetAllQuestionsFromEvent([FromQuery] int eventId)
    {
        try
        {
            List<QuestionModel> res = await _data.GetAllQuestionsFromEvent(eventId);
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