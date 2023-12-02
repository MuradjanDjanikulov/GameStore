using DataAccess.Entity;

namespace DataAccess.Repository
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {

        Task<Comment> Restore(int comment);
        Task<bool> Clear(int id);
        Task<IEnumerable<Comment>> GetAllByGame(int id);

    }

}