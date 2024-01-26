
using DataAccess.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _appDbContext;

        public CommentRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public async Task<Comment> Get(int id)
        {
            var found = await _appDbContext.Comments.
                //Where(c => !c.IsDeleted).
                Where(c => c.Id == id).
                Include(c => c.Replies).
                AsNoTracking().
                SingleOrDefaultAsync();
            //FirstOrDefaultAsync(c => c.Id == id);
            return found;
        }

        public async Task<IEnumerable<Comment>> GetAll()
        {
            var found = await _appDbContext.Comments.
                Where(c => !c.IsDeleted && c.ParentCommentId == null).
                Include(c => c.Replies).
                AsNoTracking().
                ToListAsync();
            return found;
        }

        public async Task<bool> Delete(int id)
        {
            var comment = await _appDbContext.Comments.FindAsync(id);
            if (comment != null)
            {
                comment.IsDeleted = true;
                _appDbContext.Comments.Update(comment);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> Clear(int id)
        {
            var comment = await _appDbContext.Comments.FindAsync(id);
            if (comment != null)
            {
                var childComments = _appDbContext.Comments.Where(c => c.ParentCommentId == id);
                foreach (var childComment in childComments)
                {
                    childComment.ParentCommentId = null;
                }

                _appDbContext.Comments.Remove(comment);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Comment> Create(Comment comment)
        {
            await _appDbContext.Comments.AddAsync(comment);
            await _appDbContext.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> Update(int id, Comment comment)
        {

            var found = await _appDbContext.Comments.
                Where(c => !c.IsDeleted).
                Include(c => c.Replies).
                AsNoTracking().
                FirstOrDefaultAsync(c => c.Id == id);

            if (found != null)
            {
                found.Content = comment.Content;
                _appDbContext.Comments.Update(found);
                await _appDbContext.SaveChangesAsync();
            }

            return found;
        }


        public async Task<Comment> Restore(int comment)
        {
            var found = await _appDbContext.Comments.FirstOrDefaultAsync(g => g.Id == comment);
            if (found != null)
            {
                found.IsDeleted = false;
                _appDbContext.Comments.Update(found);
                await _appDbContext.SaveChangesAsync();
            }
            return found;
        }

        public async Task<IEnumerable<Comment>> GetAllByGame(int id)
        {
            var found = await _appDbContext.Comments.
                Where(c => !c.IsDeleted && c.ParentCommentId == null && c.GameId == id).
                Include(c => c.Replies).
                AsNoTracking().
                ToListAsync();
            return found;
        }
    }

}
