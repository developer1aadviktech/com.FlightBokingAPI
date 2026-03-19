using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Dapper.SqlMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Com.Repository
{

    public class GenericRepository : IGenericRepository
    {
        //private readonly IConfiguration _configuration;
        //public IDbConnection Connection { get; }
        private readonly string connectionString;
        //private readonly IDynamicParametersService dynamicParametersService;
        //public DatabaseContext(string connectionString, IDynamicParametersService dynamicParametersService)

        //public GenericRepository(string connectionString)
        //{
        //    //Connection = new SqlConnection(connectionString);
        //    this.connectionString = connectionString;
        //    //this.dynamicParametersService = dynamicParametersService;
        //}



        //private readonly string _connectionString;
        //public GenericRepository(string connectionString)
        public GenericRepository(IConfiguration configuration)
        {
            //this.connectionString = connectionString;
            connectionString = configuration.GetConnectionString("AppDbConnectionString");
        }

        public GenericRepository(string? connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<T1> Save<T1, T2>(string spName, T2 parameters)
        {
            try
            {
                using (IDbConnection Connection = new SqlConnection(connectionString))
                {
                    // Execute the stored procedure asynchronously
                    T1 result = (await Connection.QueryAsync<T1>(
                        spName,
                        parameters,
                        commandType: CommandType.StoredProcedure
                    )).FirstOrDefault();

                    return result;
                }

            }
            catch (Exception e)
            {
                return default(T1);
            }
        }

        public async Task<IEnumerable<T>> LoadData<T, U>(string storeprocedure, U parameters, string conectionId = "")
        {

            using IDbConnection Connection = new SqlConnection(connectionString);
            return await Connection.QueryAsync<T>(storeprocedure, parameters, commandType: CommandType.StoredProcedure);

        }

        public async Task<T> LoadSingleData<T, U>(string storeprocedure, U parameters, string conectionId = "")
        {
            using IDbConnection Connection = new SqlConnection(connectionString);
            return (await Connection.QueryAsync<T>(storeprocedure, parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /*public async Task<object> LoadMultipleResultSets<T1, T2, T3, T4, T5, U>(string storeprocedure, U parameters, string conectionId = "")
        {

            using IDbConnection Connection = new SqlConnection(connectionString);
            var reader = (await Connection.QueryMultipleAsync(storeprocedure, parameters, commandType: CommandType.StoredProcedure));
            List<T1> result1 = reader.Read<T1>().ToList();
            List<T2> result2 = reader.Read<T2>().ToList();
            List<T3> result3 = reader.Read<T3>().ToList();
            List<T4> result4 = reader.Read<T4>().ToList();
            List<T5> result5 = reader.Read<T5>().ToList();
            return (new { List1 = result1, List2 = result2, List3 = result4, List5 = result5 });
        }*/

        public async Task<MultipleResult<T1, T2, T3, T4, T5>> LoadMultipleResultSets<T1, T2, T3, T4, T5, U>(
            string storedProcedure,
            U parameters,
            string connectionId = "")
        {
            using IDbConnection connection = new SqlConnection(connectionString);

            using var reader = await connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);

            return new MultipleResult<T1, T2, T3, T4, T5>
            {
                List1 = reader.Read<T1>().ToList(),
                List2 = reader.Read<T2>().ToList(),
                List3 = reader.Read<T3>().ToList(),
                List4 = reader.Read<T4>().ToList(),
                List5 = reader.Read<T5>().ToList()
            };
        }





        //public async Task<T1> SaveandReturnStatus<T1, T2>(string spName, T2 parameters)
        //{

        //    using (IDbConnection Connection = new SqlConnection(connectionString))
        //    {
        //        try
        //        {
        //            // Execute the stored procedure asynchronously
        //            T1 result = (await Connection.ExecuteScalarAsync<T1>(
        //                spName,
        //                parameters,
        //                commandType: CommandType.StoredProcedure
        //            ));
        //            return result;
        //        }
        //        catch(Exception e)
        //        {

        //            return default(T1);
        //        }
        //    }


        //}

        //public async Task<int> SaveBulk<T>(string tableName, List<T> data)
        //{
        //    using IDbConnection connection = new SqlConnection(connectionString);

        //    var parameters = new
        //    {
        //        TableName = tableName,
        //        JsonData = JsonConvert.SerializeObject(data)
        //    };

        //    return await connection.ExecuteAsync(
        //        "sp_GenericBulkInsert",
        //        parameters,
        //        commandType: CommandType.StoredProcedure);
        //}

    }

    public class MultipleResult<T1, T2, T3, T4, T5>
    {
        public List<T1> List1 { get; set; }
        public List<T2> List2 { get; set; }
        public List<T3> List3 { get; set; }
        public List<T4> List4 { get; set; }
        public List<T5> List5 { get; set; }
    }
}
