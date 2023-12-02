using DataAccess.Entity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WebAPI.Utils;

namespace WebAPI.Models
{
    public class OrderModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        [ValidEnum]
        public PaymentType PaymentType { get; set; }

        [Required]
        public ICollection<GameInfoModel> GameInfos { get; set; }

        public CommentModel? CommentModel { get; set; }

        public string? UserId { get; set; }

    }
}
