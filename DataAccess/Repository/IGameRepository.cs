﻿using DataAccess.Entity;
using Microsoft.AspNetCore.Http;

namespace DataAccess.Repository
{
    public interface IGameRepository : IGenericRepository<Game>
    {

        Task<string> SetImage(int id, IFormFile formFile);
        Task<HashSet<Genre>> GetGenres(HashSet<int> genres);
        Task<List<Game>> Search(string search);
        Task<List<Game>> Filter(int genre);

    }

}