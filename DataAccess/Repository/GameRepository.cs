
using Microsoft.EntityFrameworkCore;
using DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices;

namespace DataAccess.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _appDbContext;

        public GameRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public async Task<bool> Delete(int id)
        {
            var Game = await _appDbContext.Games.FindAsync(id);
            if (Game != null)
            {
                _appDbContext.Games.Remove(Game);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Game> Get(int id)
        {
            return await _appDbContext.Games.Include(g => g.Genres).FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Game>> GetAll()
        {
            return await _appDbContext.Games.Include(g=>g.Genres).ToListAsync();
        }


        public async Task<string> SetImage(int id, IFormFile ImageFile)
        {
            var game = await _appDbContext.Games.FindAsync(id);

            string imageDir = Directory.GetCurrentDirectory();

            string uniqueFileName = $"{game.Name}.jpeg";

            string filePath = Path.Combine(imageDir, "Images", uniqueFileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
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

        public async Task<Game> Create(Game Game)
        {
            var game = await _appDbContext.Games.AddAsync(Game);
            await _appDbContext.SaveChangesAsync();
            return Game;
        }

        public async Task<Game> Update(int id, Game game)
        {
            var found = await _appDbContext.Games.FindAsync(id);
            if (found != null) 
            { 
                _appDbContext.Entry(found).State = EntityState.Detached;
                _appDbContext.Attach(game);
                _appDbContext.Entry(game).State = EntityState.Modified;
                await _appDbContext.SaveChangesAsync();
                return game;
            }
            return null;

        }

        public async Task<HashSet<Genre>> GetGenres(HashSet<int> genres)
        {
            //List<Genre> genreList = await _appDbContext.Genres.Where(x => genres.Contains(x.Name)).ToListAsync();
            List<Genre> genreList = await _appDbContext.Genres.ToListAsync();
            var result =  new List<Genre>();
            foreach (var genre in genreList)
            {
                if (genres.Contains(genre.Id)) 
                {
                    result.Add(genre);
                }

            }
            return new HashSet<Genre>(result);
        }
    }
}
