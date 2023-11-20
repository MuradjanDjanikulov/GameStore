using DataAccess.Entity;
using WebAPI.Models;

namespace Api.Services
{

   public interface IGameService : IGenericService<GameModel, Game>
   {
        Task<string> SetImage(int id,IFormFile formFile);
        Task<List<Game>> Search(string search);
        Task<List<Game>> Filter(int genre);
   }
}
