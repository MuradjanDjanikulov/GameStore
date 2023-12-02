using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entity
{
    public class Order
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public double? TotalPrice { get; set; }

        public PaymentType PaymentType { get; set; }

        public virtual ICollection<GameInfo>? GameInfos { get; set; }

        public int? CommentId { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
    }

    [Flags]
    public enum PaymentType
    {
        Card = 1,
        Cash = 2
    }
}
