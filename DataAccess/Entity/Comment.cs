using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entity
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Username { get; set; }

        public bool IsDeleted { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        public int? GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual Game? Game { get; set; }

        public virtual ICollection<Comment>? Replies { get; set; }

    }
}
