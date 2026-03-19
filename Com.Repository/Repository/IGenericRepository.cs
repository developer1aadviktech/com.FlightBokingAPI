
namespace Com.Repository
{
    public interface IGenericRepository
    {
        Task<IEnumerable<T>> LoadData<T, U>(string storeprocedure, U parameters, string conectionId = "");
        //Task<object> LoadMultipleResultSets<T1, T2, T3, T4, T5, U>(string storeprocedure, U parameters, string conectionId = "");
        Task<MultipleResult<T1, T2, T3, T4, T5>> LoadMultipleResultSets<T1, T2, T3, T4, T5, U>( string storedProcedure,U parameters,string connectionId = "");
        Task<T> LoadSingleData<T, U>(string storeprocedure, U parameters, string conectionId = "");
        Task<T1> Save<T1, T2>(string spName, T2 parameters);
        //Task<int> SaveBulk<T>(string tableName, List<T> data);
        //object SaveBulk<T1, T2>(string v, T2 user);
    }
}