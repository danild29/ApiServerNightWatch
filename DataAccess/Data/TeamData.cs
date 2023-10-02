using Dapper;
using DataAccess.DbAccess;
using DataAccess.DbModels;
using DataAccess.DbModels.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data;

public sealed class TeamData : ITeamData
{
    private readonly ISqlDataAccess _db;

    public TeamData(ISqlDataAccess db)
    {
        _db = db;
    }


    public async Task<string?> CheckTeamPassword(int teamId, string password)
    {

        string sql = @"SELECT * FROM [dbo].[Team] WHERE @Id=Id";
        var res = (await _db.LoadData<Team, dynamic>(sql, new { Id = teamId })).FirstOrDefault();

        if (res == null) 
            return "команды с таким Id не существует";

        if (res.Password != password)
            return "Неправильно введен Id или пароль";

        return null;
    }

    public async Task<Team?> CreateTeam(CreateTeamDto team)
    {
        var p = new DynamicParameters();
        p.Add("@Id", 0, DbType.Int32, direction: ParameterDirection.Output);
        p.Add("@Name", team.Name);
        p.Add("@Password", team.Password);
        p.Add("@CaptainId", team.CaptainId);
        p.Add("@output", DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _db.SaveDataStoredProcedure("spTeam_Insert", p);

        return await GetTeam(p.Get<int>("@Id"));
    }

    public async Task DeleteTeam(int id)
    {
        string sql = @"DELETE FROM [dbo].[User] where Id = @Id;";
        await _db.SaveData(sql, new { Id = id });
    }

    public async Task<Team?> GetTeam(int id)
    {
        string sql = @"SELECT * FROM [dbo].[Team] WHERE Id=@Id";
        var res = await _db.LoadData<Team, dynamic>(sql, new { Id = id });

        return res.FirstOrDefault();
    }
    public async Task<Team?> GetTeamByUserId(int userId)
    {
        string sql = @"Select * from [dbo].[Team] Where Id = (select TeamId from [dbo].[User] Where Id = @Id)";

        var res = await _db.LoadData<Team, dynamic>(sql, new { Id = userId });

        return res.FirstOrDefault();
    }

    public async Task<IEnumerable<Team>> GetTeams()
    {
        string sql = @"SELECT * FROM [dbo].[Team]";

        return await _db.LoadData<Team, dynamic>(sql, new { });
    }

    public async Task<IEnumerable<Team>> GetTeamsByEventId(int eventId)
    {
        string sql = @"Select * from [dbo].[Team] Where EventId = @EventId";

        var res = await _db.LoadData<Team, dynamic>(sql, new { EventId = eventId });

        return res;
    }


    public async Task UpdateTeam<T>(int id, string field, T? value)
    {
        PropertyInfo[] propertyInfos = typeof(Team).GetProperties();
        if (!propertyInfos.Any(x => x.Name == field)) throw new Exception("u using this method wrong");

        string sql = $"UPDATE [dbo].[Team] set {field} = @Value WHERE @Id=id";
        await _db.LoadData<Team, dynamic>(sql, new { Id = id, Value = value });
    }
}

public interface ITeamData
{
    Task<Team?> GetTeam(int id);
    Task<Team?> GetTeamByUserId(int userId);
    Task<IEnumerable<Team>> GetTeamsByEventId(int eventId);
    Task<IEnumerable<Team>> GetTeams();
    Task<Team?> CreateTeam(CreateTeamDto team);
    Task<string?> CheckTeamPassword(int teamId, string password);
    Task DeleteTeam(int id);
    Task UpdateTeam<T>(int id, string field, T? value);
}
