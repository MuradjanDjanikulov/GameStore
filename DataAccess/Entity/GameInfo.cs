using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entity
{
    public class GameInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Amount { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order order { get; set; }

    }
}
