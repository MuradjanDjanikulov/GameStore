using DataAccess.Entity;
using WebAPI.Models;

namespace Api.Services
{

    public interface ICommentService : IGenericService<CommentModel, Comment>
    {
        Task<Comment> Restore(int comment);
        Task<bool> Clear(int id);
        Task<IEnumerable<Comment>> GetAllByGame(int id);

    }
}
