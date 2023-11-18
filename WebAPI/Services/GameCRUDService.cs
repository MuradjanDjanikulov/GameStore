using Api.Models;
using DataAccess.Entity;
using DataAccess.Repository;
using WebAPI.Models;


namespace Api.Services
{
    public class GameCRUDService : IGameCRUDService
    {
        private readonly IGameRepository _gameRepository;

        public GameCRUDService(IGameRepository GameRepository)
        {
            _gameRepository = GameRepository;
        }

        
        public async Task<bool> Delete(int id)
        {
            return await _gameRepository.Delete(id);

        }

        public async Task<Game> Get(int id)
        {
            var game = await _gameRepository.Get(id);
            
            return game;
        }

        public async Task<IEnumerable<Game>> GetAll()
        {
            var games = await _gameRepository.GetAll();
            return games;
        }


        public async Task<string> SetImage(int id, IFormFile ImageFile)
        {
            var imageUrl = await _gameRepository.SetImage(id, ImageFile);
            return imageUrl;
        }

        public async Task<Game> Create(GameModel model)
        {
            var genres = await _gameRepository.GetGenres(model.Genres);
            var game = new Game { Name = model.Name, Description = model.Description, Price = model.Price, Genres = genres };
            var createdGame = await _gameRepository.Create(game);

            return createdGame;

        }
        
        public async Task<Game> Update(int id, GameModel model)
        {
            var genres = await _gameRepository.GetGenres(model.Genres);
            var game = new Game { Id = id, Name = model.Name, Description = model.Description, Price = model.Price, Genres = genres };
            var updatedGame = await _gameRepository.Update(id, game);
            return updatedGame;
        }

    }
}
