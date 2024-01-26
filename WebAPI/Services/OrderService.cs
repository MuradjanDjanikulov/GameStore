using DataAccess.Entity;
using DataAccess.Repository;
using WebAPI.Models;


namespace Api.Services
{
    public class OrderService : IGenericService<OrderModel, Order>
    {
        private readonly IGenericRepository<Order> _orderRepository;

        public OrderService(IGenericRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> Delete(int id)
        {
            return await _orderRepository.Delete(id);
        }

        public async Task<Order> Get(int id)
        {
            return await _orderRepository.Get(id);

        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _orderRepository.GetAll();
        }


        public async Task<Order> Create(OrderModel model)
        {
            List<GameInfo> games = new List<GameInfo>();
            foreach (var item in model.GameInfos)
            {
                var gameInfo = new GameInfo { GameId = item.Id, Amount = item.Amount };
                games.Add(gameInfo);
            }
            var order = new Order
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                PaymentType = model.PaymentType,
                GameInfos = games,
                UserId = model.UserId
            };

            Comment comment;
            if (model.CommentModel != null)
            {
                comment = new Comment
                {
                    Content = model.CommentModel.Content,
                    Username = model.CommentModel.Username,
                    CreatedDate = DateTime.UtcNow,
                    UserId = model.UserId,
                };
                order.Comment = comment;
            }

            var created = await _orderRepository.Create(order);

            return created;

        }

        public async Task<Order> Update(int id, OrderModel model)
        {
            List<GameInfo> games = new List<GameInfo>();
            foreach (var item in model.GameInfos)
            {
                var gameInfo = new GameInfo { GameId = item.Id, Amount = item.Amount };
                games.Add(gameInfo);
            }

            var order = new Order
            {
                FirstName = model.FirstName,
                LastName = model.FirstName,
                Email = model.Email,
                Phone = model.Phone,
                PaymentType = model.PaymentType,
                GameInfos = games,

            };

            Comment comment;

            if (model.CommentModel != null)
            {
                comment = new Comment
                {
                    Content = model.CommentModel.Content,
                };
                order.Comment = comment;
            }

            var updatedOrder = await _orderRepository.Update(id, order);
            return updatedOrder;
        }

    }
}
