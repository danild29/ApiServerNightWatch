using Dapper;
using DataAccess.DbAccess;
using DataAccess.DbModels;


namespace DataAccess.Data;


public sealed class QuestionData : IQuestionData
{
    private readonly ISqlDataAccess _db;

    public QuestionData(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<List<QuestionModel>> GetAllQuestionsFromEvent(int id)
    {
        string sql = @"SELECT * FROM [dbo].[Question] WHERE EventId = @Id";
        var res = await _db.LoadData<QuestionModel, dynamic>(sql, new { Id = id });

        return res.ToList();
    }



    public async Task<QuestionModel?> CreateQuestion(QuestionModel que)
    {
        string sql = "INSERT INTO[dbo].[Question] (Answer,  EventId, Content, ContentType, Question) " +
                                          "values(@Answer, @EventId, @Content, @ContentType, @Question);";

        await _db.SaveData(sql, new { que.Answer, que.EventId, que.Content, que.ContentType, que.Question});
        return que;
    }

    //public async Task<QuestionModel?> CreateQuestionWithSP(QuestionModel que)
    //{

    //    byte[] image = File.ReadAllBytes(@"C:\Users\Dania\source\repos\ApiServerNightWatch\DataAccess\Data\test.jpg");

    //    var p = new DynamicParameters();
    //    p.Add("@Id", 0, DbType.Int32, direction: ParameterDirection.Output);
    //    p.Add("@Answer", que.Answer);
    //    p.Add("@ContentType", "img");
    //    p.Add("@Content", image);
    //    p.Add("@EventId", 2);
    //    p.Add("@output", DbType.Int32, direction: ParameterDirection.ReturnValue);

    //    await _db.SaveDataStoredProcedure("spEvent_Insert", p);

    //    return await GetQuestion(p.Get<int>("@Id"));

    //}

}


public interface IQuestionData
{
    Task<QuestionModel?> CreateQuestion(QuestionModel que);
    Task<List<QuestionModel>> GetAllQuestionsFromEvent(int id);
}