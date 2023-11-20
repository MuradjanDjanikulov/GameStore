using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entity
{
    public class Genre
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentGenreId { get; set; }

        [ForeignKey("ParentGenreId")]
        public virtual Genre? ParentGenre { get; set; }

        public virtual ICollection<Game>? Games { get; set; }

        [InverseProperty("ParentGenre")]
        public virtual ICollection<Genre>? SubGenres { get; set; }

    }
}
