using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Reflection;

namespace CrudOperations
{
    public class CrudOperationDataAccess(IConfiguration configuration) 
    {
        private readonly IConfiguration _configuration = configuration;

        public IDbConnection Connection =>
            new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        #region Session Context (Postgres)

        private async Task SetPgSessionAsync(IDbConnection connection)
        {
            // if (connection.State != ConnectionState.Open)
            //     await connection.OpenAsync();

            // PostgreSQL session variables
            var sql = @"
                SELECT set_config('app.user_id', @user_id, false);
                SELECT set_config('app.user_ip', @user_ip, false);
            ";

            await connection.ExecuteAsync(sql, new
            {
                user_id = "system",
                user_ip = "127.0.0.1"
            });
        }

        #endregion

        #region Execute Helpers

        private static string BuildFunctionCall(string functionName, object paramObj)
        {
            var props = paramObj?.GetType().GetProperties() ?? [];
            var args = string.Join(",", props.Select(p => "@" + p.Name));
            return $"SELECT * FROM {functionName}({args});";
        }

        #endregion

        #region Insert / Update / Delete

        public async Task<T> ExecuteSingleAsync<T>(string functionName, object parameters)
        {
            using var conn = Connection;
            await SetPgSessionAsync(conn);

            var sql = BuildFunctionCall(functionName, parameters);

            return await conn.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        #endregion

        #region Get Single Record

        public async Task<Response<T>> GetSingleRecord<T>(string functionName, object parameters)
        {
            using var conn = Connection;
            await SetPgSessionAsync(conn);

            var sql = BuildFunctionCall(functionName, parameters);

            using var multi = await conn.QueryMultipleAsync(sql, parameters);

            var response = await multi.ReadFirstOrDefaultAsync<Response<T>>();
            if (response?.Status == true)
                response.Data = await multi.ReadFirstOrDefaultAsync<T>();

            return response;
        }

        #endregion

        #region Get List

        public async Task<ResponseList<T>> GetList<T>(string functionName, object parameters)
        {
            using var conn = Connection;
            await SetPgSessionAsync(conn);

            var sql = BuildFunctionCall(functionName, parameters);

            using var multi = await conn.QueryMultipleAsync(sql, parameters);

            var response = await multi.ReadFirstOrDefaultAsync<ResponseList<T>>();
            if (response?.Status == true)
                response.Data = (await multi.ReadAsync<T>()).ToList();

            return response;
        }

        #endregion

        #region Paginated List

        public async Task<ResponseList<T>> GetPaginatedList<T>(string functionName, object parameters)
        {
            using var conn = Connection;
            await SetPgSessionAsync(conn);

            var sql = BuildFunctionCall(functionName, parameters);

            using var multi = await conn.QueryMultipleAsync(sql, parameters);

            var response = await multi.ReadFirstOrDefaultAsync<ResponseList<T>>();
            if (response?.Status == true)
            {
                response.Data = (await multi.ReadAsync<T>()).ToList();
                response.TotalRecords = response.RecordsFiltered =
                    await multi.ReadFirstOrDefaultAsync<int>();
            }

            return response;
        }

        #endregion

        #region Manual Mapper

        public async Task ManualListMapperAsync(
            string functionName,
            Action<SqlMapper.GridReader> callback,
            object parameters = null)
        {
            using var conn = Connection;
            await SetPgSessionAsync(conn);

            var sql = BuildFunctionCall(functionName, parameters);

            using var multi = await conn.QueryMultipleAsync(sql, parameters);
            callback(multi);
        }

        #endregion
    }
}