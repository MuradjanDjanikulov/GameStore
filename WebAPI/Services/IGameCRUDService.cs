using DataAccess.Entity;
using WebAPI.Models;

namespace Api.Services
{

   public interface IGameCRUDService : IGenericCRUDService<GameModel, Game>
   {

    Task<string> SetImage(int id,IFormFile formFile);

   }
}
