using DataAccess.Entity;
using DataAccess.Repository;
using WebAPI.Models;


namespace Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository CommentRepository)
        {
            _commentRepository = CommentRepository;
        }


        public async Task<bool> Delete(int id)
        {
            return await _commentRepository.Delete(id);

        }

        public async Task<bool> Clear(int id)
        {
            return await _commentRepository.Clear(id);

        }

        public async Task<Comment> Get(int id)
        {
            return await _commentRepository.Get(id);

        }

        public async Task<IEnumerable<Comment>> GetAll()
        {
            return await _commentRepository.GetAll();
        }


        public async Task<Comment> Create(CommentModel model)
        {
            var comment = new Comment
            {
                Content = model.Content,
                CreatedDate = DateTime.UtcNow,
                Username = model.Username,
                IsDeleted = false,
                UserId = model.UserId,
                ParentCommentId = model.ParentCommentId,
                GameId = model.GameId
            };
            var created = await _commentRepository.Create(comment);

            return created;

        }

        public async Task<Comment> Update(int id, CommentModel model)
        {
            var comment = new Comment { Id = id, Content = model.Content };
            var updatedComment = await _commentRepository.Update(id, comment);
            return updatedComment;
        }

        public async Task<Comment> Restore(int comment)
        {
            return await _commentRepository.Restore(comment);
        }

    }
}
