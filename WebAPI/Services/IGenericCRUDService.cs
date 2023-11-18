namespace Api.Services
{

   public interface IGenericCRUDService<T, K> where T : class where K : class
    {
    Task<IEnumerable<K>> GetAll();

    Task<K> Get(int id);

    Task<K> Create(T employee);

    Task<K> Update(int id, T employee);

    Task<bool> Delete(int id);

   }
}
