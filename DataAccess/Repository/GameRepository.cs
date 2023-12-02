
using DataAccess.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _appDbContext;

        public GameRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public async Task<Game> Get(int id)
        {
            var found = await _appDbContext.Games.
                Include(g => g.Genres).
                Include(g => g.Comments).
                AsNoTracking().
                FirstOrDefaultAsync(g => g.Id == id);

            return found;
        }

        public async Task<Game> GetByName(String name)
        {
            var found = await _appDbContext.Games.FirstOrDefaultAsync(g => g.Name == name);

            return found;
        }

        public async Task<IEnumerable<Game>> GetAll()
        {
            var found = await _appDbContext.Games.
                Include(g => g.Genres).
                Include(g => g.Comments).
                AsNoTracking().
                ToListAsync();

            return found;
        }

        public async Task<bool> Delete(int id)
        {
            var game = await _appDbContext.Games.FindAsync(id);
            if (game != null)
            {
                _appDbContext.Games.Remove(game);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<string> SetImage(int id, IFormFile ImageFile)
        {
            var game = await _appDbContext.Games.FindAsync(id);

            string imageDir = Directory.GetCurrentDirectory();

            string uniqueFileName = $"{game.Name}.jpeg";

            string filePath = Path.Combine(imageDir, "Images", uniqueFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            game.ImageUrl = filePath;

            var updatedGame = _appDbContext.Games.Attach(game);
            updatedGame.State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();

            return filePath;

        }

        public async Task<Game> Create(Game game)
        {
            var found = await _appDbContext.Games.Where(g => g.Name.Equals(game.Name)).FirstOrDefaultAsync();
            if (found == null)
            {
                await _appDbContext.Games.AddAsync(game);
                await _appDbContext.SaveChangesAsync();
                return game;
            }
            return null;
        }

        public async Task<Game> Update(int id, Game game)
        {
            //var found = await _appDbContext.Games.Include(g => g.Genres).FirstOrDefaultAsync(g => g.Id == id);

            var found = await _appDbContext.Games.FindAsync(id);
            if (found != null)
            {
                await _appDbContext.Entry(found).Collection(g => g.Genres).LoadAsync();

                found.Genres?.Clear();

                found.Name = game.Name;
                found.Description = game.Description;
                found.Genres = game.Genres;
                found.Price = game.Price;
                found.ImageUrl = game.ImageUrl;
                found.Genres = game.Genres;
                await _appDbContext.SaveChangesAsync();
            }

            return found;
        }

        public async Task<List<Genre>> GetGenres(HashSet<int> genres)
        {

            var genreList = await _appDbContext.Genres.Where(genre => genres.Contains(genre.Id)).ToListAsync();

            return genreList;

        }

        public async Task<List<Game>> Search(string search)
        {
            var searched = await _appDbContext.Games.
                Where(game => game.Name.StartsWith(search)).
                Include(g => g.Genres).
                Include(g => g.Comments).
                AsNoTracking().
                ToListAsync();

            if (searched != null)
            {
                return searched;
            }
            return new List<Game>();
        }

        public async Task<List<Game>> Filter(int genre)
        {
            var found = await _appDbContext.Genres.FirstOrDefaultAsync(g => g.Id == genre);
            if (found != null)
            {
                var result = await _appDbContext.Games.
                    Where(g => g.Genres.Any(g => g.Equals(found) || g.ParentGenre.Equals(found)))
                    .Include(g => g.Genres)
                    .Include(g => g.Comments)
                    .AsNoTracking()
                    .ToListAsync();

                return result;
            }
            return new List<Game>();
        }
    }
}
