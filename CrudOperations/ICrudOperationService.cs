using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrudOperations
{
    public interface ICrudOperationService
    {
        string GetConnectionString(string connectionName);

        string AppSettingsKeys(string nodeName, string key);

        IConfigurationSection GetConfigurationSection(string key);

        /// <summary>
        /// This Method will Insert data into table and return Status, Message, Data
        /// </summary>
        /// <typeparam name="T">Place your Response class for Binding Proc response returns</typeparam>
        /// <param name="storedProcedureName">Your Procedure name</param>
        /// <param name="parameters">Paramters for procedure</param>
        /// <returns>Genric Type T Passed in PatientInsert</returns>
        Task<T> Insert<T>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        /// This Method will Insert data into table and return Status, Message, Data (Of T type)
        /// </summary>
        /// <typeparam name="T"> Type of returned data </typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parametersPocoObj"></param>
        /// <returns></returns>
        Task<Response<T>> InsertAndGet<T>(string storedProcedureName, object parametersPocoObj);

        /// <summary>
        /// This Method will Update data into table and return Status, Message, Data
        /// </summary>
        /// <typeparam name="T">Place your Response class for Binding Proc response returns</typeparam>
        /// <param name="storedProcedureName">Your Procedure name</param>
        /// <param name="parameters">Paramters for procedure</param>
        /// <returns>Genric Type T Passed in PatientUpdate</returns>
        Task<T> Update<T>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        /// This Method will Delete data into table and return Status, Message, Data
        /// </summary>
        /// <typeparam name="T">Place your Response class for Binding Proc response returns</typeparam>
        /// <param name="storedProcedureName">Your Procedure name</param>
        /// <param name="parameters">Paramters for procedure</param>
        /// <returns>Genric Type T Supplied in PatientDelete</returns>
        Task<T> Delete<T>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        /// This function can be used to perform insert update delete transactions
        /// </summary>
        /// <typeparam name="T">Class to cast db response</typeparam>
        /// <param name="storedProcedureName">your procedure name</param>
        /// <param name="spParamsPocoMapper">SP based model class</param>
        /// <returns>procedure return message casted in class</returns>
        Task<T> InsertUpdateDelete<T>(string storedProcedureName, object spParamsPocoMapper);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<Response<T>> GetSingleRecord<T>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="spParamsPocoMapper"></param>
        /// <returns></returns>
        Task<Response<T>> GetSingleRecord<T>(string storedProcedureName, object spParamsPocoMapper);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <returns></returns>
        Task<Response<T>> GetSingleRecord<T>(string storedProcedureName);

        /// <summary>
        /// Get Single record with addition list of data
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<Response<Tuple<T1, List<T2>>>> GetSingleRecord<T1, T2>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        /// Get Single record with addition list of data
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<Response<Tuple<T1, List<T2>>>> GetSingleRecord<T1, T2>(string storedProcedureName, object parameters);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ResponseList<T>> GetList<T>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="spParamsPocoMapper"></param>
        /// <returns></returns>
        Task<ResponseList<T>> GetList<T>(string storedProcedureName, object spParamsPocoMapper);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ResponseList<T>> GetPaginatedList<T>(string storedProcedureName, DynamicParameters parameters);

        /// <summary>
        /// Genric Class based List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="spParamsPocoMapper"></param>
        /// <returns></returns>
        Task<ResponseList<T>> GetPaginatedList<T>(string storedProcedureName, object spParamsPocoMapper);

        Task ManualListMapperAsync(string storedProcedureName, Action<SqlMapper.GridReader> callback, object parameters = null);

        Task<ResponseListWithIds<T>> GetPaginatedListWithClientIds<T>(string storedProcedureName, object spParamsPocoMapper);


    }
}