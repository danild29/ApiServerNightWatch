using Dapper;
using DataAccess.DbModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbAccess;

public class SqlDataAccess : ISqlDataAccess
{
    private readonly IConfiguration _config;

    public SqlDataAccess(IConfiguration configuration)
    {
        _config = configuration;
    }

    public async Task<IEnumerable<T>> LoadData<T, U>(string sql, U parametrs, string connectionId = "Default")
    {

        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

        return await connection.QueryAsync<T>(sql, parametrs);
    }


    public async Task SaveData<T>(string sql, T parametrs, string connectionId = "Default")
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

        await connection.ExecuteAsync(sql, parametrs);
    }


    public async Task<IEnumerable<T>> LoadDataStoredProcedure<T, U>(string storedProcedure, U parametrs, string connectionId = "Default")
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

        return await connection.QueryAsync<T>(storedProcedure, parametrs, commandType: CommandType.StoredProcedure);
    }


    public async Task SaveDataStoredProcedure<T>(string storedProcedure, T parametrs, string connectionId = "Default")
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

        await connection.ExecuteAsync(storedProcedure, parametrs, commandType: CommandType.StoredProcedure);
    }



   


}

public interface ISqlDataAccess
{
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parametrs, string connectionId = "Default");
    Task SaveData<T>(string storedProcedure, T parametrs, string connectionId = "Default");
    Task<IEnumerable<T>> LoadDataStoredProcedure<T, U>(string storedProcedure, U parametrs, string connectionId = "Default");
    Task SaveDataStoredProcedure<T>(string storedProcedure, T parametrs, string connectionId = "Default");
}