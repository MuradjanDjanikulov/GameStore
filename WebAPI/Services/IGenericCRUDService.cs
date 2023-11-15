using Microsoft.AspNetCore.JsonPatch;
namespace Api.Services
{
    public interface IGenericCRUDService <T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<T> Get(int id);

        Task<T> Create(T employee);

        Task<T> Update(int id, T employee);

        Task<bool> Delete(int id);

//        Task<T> UpdatePatch(int id, JsonPatchDocument employee);

    }
}
