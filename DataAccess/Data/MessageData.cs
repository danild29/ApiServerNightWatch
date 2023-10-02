using Dapper;
using DataAccess.DbAccess;
using DataAccess.DbModels.Dtos;
using DataAccess.DbModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace DataAccess.Data;

public interface IMessageData
{
    Task<int> SaveMesssage(MessageModel message);

    Task<List<int>> GetCompanions(int id);
    Task<IEnumerable<MessageModel>> GetNewMessages(int index1, int index2, int lastMessageId = 0);
}



public class MessageData : IMessageData
{
    private readonly ISqlDataAccess _db;

    public MessageData(ISqlDataAccess db) => _db = db;


    public async Task<int> SaveMesssage(MessageModel message)
    {
        var p = new DynamicParameters();
        p.Add("@Id", 0, DbType.Int32, direction: ParameterDirection.Output);
        p.Add("@FromTeam", message.FromTeam);
        p.Add("@ToEvent", message.ToEvent);
        p.Add("@Content", message.Content);
        p.Add("@CreatedAt", message.CreatedAt);
        p.Add("@output", DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _db.SaveDataStoredProcedure("spMessage_Insert", p);

        return p.Get<int>("@Id");

    }

    public async Task<IEnumerable<MessageModel>> GetNewMessages(int index1, int index2, int lastMessageId = 0)
    {
        string sql = @"SELECT * FROM [dbo].[Message] WHERE (FromTeam = @Index1 AND ToEvent = @Index2) OR (FromTeam = @Index2 AND ToEvent = @Index1) AND Id > @LastMessageId";
        return await _db.LoadData<MessageModel, dynamic>(sql, new { Index1 = index1, Index2 = index2, LastMessageId = lastMessageId });
    }


    public async Task<List<int>> GetCompanions(int id)
    {
        string sql = @"SELECT DISTINCT FromTeam, ToEvent FROM [dbo].[Message] WHERE (FromTeam = @Id OR ToEvent = @Id)";
        var res = await _db.LoadData<MessagngersAquintances, dynamic>(sql, new { Id = id});


        List<int> ints = new List<int>();
        foreach (var item in res)
        {
            if(item.FromTeam == id) ints.Add(item.ToEvent);
            else ints.Add(item.FromTeam);
        }

        return ints;
    }
}

public class MessagngersAquintances
{
    public int FromTeam { get; set; }
    public int ToEvent { get; set; }
}

