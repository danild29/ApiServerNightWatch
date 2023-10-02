using Dapper;
using DataAccess.DbAccess;
using DataAccess.DbModels;
using DataAccess.DbModels.Dtos;
using System.Data;
using System.Reflection;

namespace DataAccess.Data;


public class UserData : IUserData
{
    private readonly ISqlDataAccess _db;

    public UserData(ISqlDataAccess db) => _db = db;
   

    public async Task<User?> Login(UserLoginDto user)
    {
        string sql = @"SELECT * FROM [dbo].[User] WHERE Email = @Email and Password = @Password";
        var res = await _db.LoadData<User, dynamic>(sql, new { user.Email, user.Password});

        return res.FirstOrDefault();
    }

    public async Task<User?> Register(UserRegisterDto user)
    {
        
        var p = new DynamicParameters();
        p.Add("@Id", 0, DbType.Int32, direction: ParameterDirection.Output);
        p.Add("@Name", user.Name);
        p.Add("@Email", user.Email);
        p.Add("@Password", user.Password);
        p.Add("@output", DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _db.SaveDataStoredProcedure("spUser_Insert", p);

        return await GetUser(p.Get<int>("@Id"));
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        string sql = @"SELECT * FROM [dbo].[User]";

        return await _db.LoadData<User, dynamic>(sql, new { });
    }

    public async Task<IEnumerable<User>> GetAllTeamates(int teamId)
    {
        string sql = @"SELECT Id FROM [dbo].[User] WHERE TeamId = @TeamId";

        return await _db.LoadData<User, dynamic>(sql, new { TeamId  = teamId});
    }

    public async Task<User?> GetUser(int id)
    {
        string sql = @"SELECT * FROM [dbo].[User] WHERE Id = @Id";
        var res = await _db.LoadData<User, dynamic>(sql, new { Id = id });

        return res.FirstOrDefault();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id">у какого юзера меняем занчение</param>
    /// <param name="field">поле в бд</param>
    /// <param name="value">на что меняем</param>
    /// <returns></returns>
    public async Task UpdateUser<T>(int id, string field, T? value)
    {
        PropertyInfo[] propertyInfos = typeof(User).GetProperties();
        if (!propertyInfos.Any(x => x.Name == field)) throw new Exception("u using this method wrong"); 
            

        string sql = $"UPDATE [dbo].[User] set {field} = @Value WHERE Id = @Id";
        await _db.LoadData<User, dynamic>(sql, new { Id = id, Value = value });
    }
}


public interface IUserData
{
    Task<User?> Register(UserRegisterDto user);
    Task<User?> Login(UserLoginDto user);
    Task UpdateUser<T>(int id, string field, T value);
    Task<User?> GetUser(int id);
    Task<IEnumerable<User>> GetUsers();
    Task<IEnumerable<User>> GetAllTeamates(int teamId);
}
