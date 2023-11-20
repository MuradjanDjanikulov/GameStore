using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class CommentModel
    {
        [Required]
        [StringLength(600, ErrorMessage = "The {0} must be at least {1} character long.", MinimumLength = 1)]
        public string Content { get; set; }

        public string? Username { get; set; }

        public string? UserId { get; set; }

        public int? ParentCommentId { get; set; }

        public int GameId { get; set; }


    }
}
