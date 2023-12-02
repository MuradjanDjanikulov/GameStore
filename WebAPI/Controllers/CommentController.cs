using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Models;

namespace Api.Controllers
{

    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentSvc;
        public CommentController(ICommentService commentSvc)
        {
            _commentSvc = commentSvc;
        }

        [HttpGet]
        [Route("get-all-comments")]
        [Authorize(Policy = "RequireManager")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _commentSvc.GetAll());
        }

        [HttpGet]
        [Route("get-all-comments-by-game/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllByGame([FromRoute] int id)
        {
            return Ok(await _commentSvc.GetAllByGame(id));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("get-comment/{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var found = await _commentSvc.Get(id);
            if (found != null)
            {
                return Ok(found);
            }
            return NotFound(new ResponseModel("Error", "Comment not found"));

        }

        [HttpPost]
        [Route("create-comment")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] CommentModel comment)
        {
            comment.Username = "anonymous";
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                comment.UserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                comment.Username = HttpContext.User.Identity.Name;
            }

            var created = await _commentSvc.Create(comment);
            if (created != null)
            {
                var routeValue = new { id = created.Id };
                return CreatedAtRoute(routeValue, created);
            }
            return BadRequest(new ResponseModel("Error", "Comment not created"));
        }

        [HttpPut]
        [Authorize]
        [Route("update-comment/{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] CommentModel comment)
        {

            if (comment.UserId.Equals(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                var updated = await _commentSvc.Update(id, comment);
                if (updated != null)
                {
                    return Ok(updated);
                }

            }
            return NotFound(new ResponseModel("Error", "Comment not found"));
        }

        [HttpPut]
        [Authorize]
        [Route("remove-comment/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            bool result = await _commentSvc.Delete(id);
            if (result) { return Ok(new ResponseModel("Success", "Comment removed successfully")); }
            return NotFound(new ResponseModel("Error", "Game not found"));
        }

        [HttpPut]
        [Authorize]
        [Route("restore-comment/{id}")]
        public async Task<IActionResult> Restore([FromRoute] int id)
        {
            var found = await _commentSvc.Get(id);
            if (found != null && found.UserId.Equals(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                var restored = await _commentSvc.Restore(id);
                if (restored != null)
                {
                    return Ok(restored);
                }
            }

            return BadRequest(new ResponseModel("Error", "Comment not found"));
        }

        [HttpDelete]
        [Authorize(Policy = "RequireManager")]
        [Route("clear-comment/{id}")]
        public async Task<IActionResult> Clear([FromRoute] int id)
        {
            bool result = await _commentSvc.Clear(id);
            if (result) { return Ok(new ResponseModel("Success", "Comment removed successfully")); }
            return NotFound(new ResponseModel("Error", "Game not found"));
        }
    }
}

