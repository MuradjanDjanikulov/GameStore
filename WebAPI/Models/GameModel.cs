using DataAccess.Entity;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class GameModel
    {

        [Required]
        public string Name { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public virtual HashSet<int>? Genres { get; set; }
    }
}
