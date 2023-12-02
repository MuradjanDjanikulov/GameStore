
using DataAccess.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class OrderRepository : IGenericRepository<Order>
    {
        private readonly AppDbContext _appDbContext;

        public OrderRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Order> Get(int id)
        {
            var found = await _appDbContext.Orders.Include(c => c.Comment).Include(c => c.GameInfos).AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

            return found;
        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            var found = await _appDbContext.Orders.Include(c => c.Comment).Include(c => c.GameInfos).AsNoTracking().ToListAsync();

            return found;
        }

        public async Task<bool> Delete(int id)
        {
            var order = await _appDbContext.Orders.FindAsync(id);
            if (order != null)
            {
                _appDbContext.Orders.Remove(order);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }


        public async Task<Order> Create(Order order)
        {
            var gameInfo = order.GameInfos.ToList();

            var games = await _appDbContext.Games.ToListAsync();

            double totalPrice = gameInfo.Where(info => games.Any(game => game.Name.Equals(info.Name))).Sum(info => info.Amount * games.First(game => game.Name.Equals(info.Name)).Price);

            order.GameInfos = gameInfo.Where(info => games.Any(game => game.Name.Equals(info.Name))).ToList();


            order.TotalPrice = totalPrice;

            await _appDbContext.Orders.AddAsync(order);
            await _appDbContext.SaveChangesAsync();
            return order;
        }

        public async Task<Order> Update(int id, Order order)
        {

            var found = await _appDbContext.Orders.Include(o => o.Comment).Include(o => o.GameInfos).AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (found != null)
            {
                _appDbContext.GameInfos.RemoveRange(found.GameInfos);

                var games = await _appDbContext.Games.ToListAsync();

                var gameInfo = order.GameInfos.ToList();

                gameInfo = gameInfo.Where(info => games.Any(g => g.Name.Equals(info.Name))).ToList();

                double totalPrice = gameInfo.Join(games, info => info.Name, game => game.Name, (info, game) => info.Amount * game.Price).Sum();

                found.TotalPrice = totalPrice;
                found.FirstName = order.FirstName;
                found.LastName = order.FirstName;
                found.Email = order.Email;
                found.Phone = order.Phone;
                found.PaymentType = order.PaymentType;

                found.GameInfos.Clear();
                Console.WriteLine(found.GameInfos);
                found.GameInfos = order.GameInfos;
                if (order.Comment != null)
                {
                    found.Comment.Content = order.Comment.Content;
                }
                _appDbContext.Update(found);

                await _appDbContext.SaveChangesAsync();
            }

            return found;
        }


    }
}
