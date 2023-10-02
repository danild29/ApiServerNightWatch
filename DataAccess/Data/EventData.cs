using Dapper;
using DataAccess.DbAccess;
using DataAccess.DbModels.Dtos;
using DataAccess.DbModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DataAccess.Data;

public sealed class EventData : IEventData
{
    private readonly ISqlDataAccess _db;

    public EventData(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<EventModel?> CreateEvent(EventModel e)
    {
        var p = new DynamicParameters();
        p.Add("@Id", 0, DbType.Int32, direction: ParameterDirection.Output);
        p.Add("@Name", e.Name);
        p.Add("@Password", e.Password);
        p.Add("@CreatorId", e.CreatorId);
        p.Add("@StartAt", e.StartAt);
        p.Add("@Duration", e.Duration);
        p.Add("@Description", e.Description);
        p.Add("@output", DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _db.SaveDataStoredProcedure("spEvent_Insert", p);

        return await GetEvent(p.Get<int>("@Id"));
    }

    public async Task<EventModel?> GetEvent(int id)
    {
        string sql = @"SELECT * FROM [dbo].[Event] WHERE @Id=Id";
        var res = await _db.LoadData<EventModel, dynamic>(sql, new { Id = id });

        return res.FirstOrDefault();
    }
    
    public async Task<EventModel?> GetEventByUserId(int userId)
    {
        string sql = @"SELECT * FROM [dbo].[Event] WHERE Id = (SELECT TOP 1 EventId FROM [dbo].[Team] WHERE Id = (SELECT TOP 1 TeamId FROM [dbo].[User] WHERE @Id = Id))";
        var res = await _db.LoadData<EventModel, dynamic>(sql, new { Id = userId });

        return res.FirstOrDefault();
    }


    public async Task<EventModel?> GetEventInfoByCreator(int creatorId)
    {
        string sql = @"SELECT * FROM [dbo].[Event] WHERE CreatorId = @Id";
        var res = await _db.LoadData<EventModel, dynamic>(sql, new { Id = creatorId });

        return res.FirstOrDefault();
    }


    public async Task UpdateEvent<T>(int id, string field, T? value)
    {
        PropertyInfo[] propertyInfos = typeof(EventModel).GetProperties();
        if (!propertyInfos.Any(x => x.Name == field)) throw new Exception("u using this method wrong");


        string sql = $"UPDATE [dbo].[Event] set {field} = @Value WHERE @Id=id";
        await _db.LoadData<EventModel, dynamic>(sql, new { Id = id, Value = value });
    }
}


public interface IEventData
{
    Task<EventModel?> CreateEvent(EventModel e);
    Task<EventModel?> GetEvent(int id);
    Task<EventModel?> GetEventByUserId(int UserId);
    Task<EventModel?> GetEventInfoByCreator(int creatorId);
    Task UpdateEvent<T>(int id, string field, T? value);
}