using DataAccess.Entity;

namespace DataAccess.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<T> Get(int id);

        Task<T> Create(T type);

        Task<T> Update(int id, T type);

        Task<bool> Delete(int id);

        //        Task<T> UpdatePatch(int id, JsonPatchDocument type);

        /*    {"path": "development",
             "op": "replace",
             "value": "ZZZ"}
       */

    }

}