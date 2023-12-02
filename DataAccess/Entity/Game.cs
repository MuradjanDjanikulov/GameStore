namespace DataAccess.Entity
{
    public class Game
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public double Price { get; set; }

        public virtual HashSet<Genre>? Genres { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
