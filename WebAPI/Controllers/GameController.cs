using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace Api.Controllers
{

    [Route("api/game")]//[controller]/[action]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameSvc;

        public GameController(IGameService gameSvc)
        {
            _gameSvc = gameSvc;
        }

        [HttpGet]
        [Route("get-all-games")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _gameSvc.GetAll());
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("get-game/{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var found = await _gameSvc.Get(id);
            if (found != null)
            {
                return Ok(found);
            }
            return NotFound(new ResponseModel("Error", "Game not found"));


        }

        [HttpPost]
        [Authorize(Policy = "RequireManager")]
        [Route("create-game")]
        public async Task<IActionResult> Post([FromBody] GameModel game)
        {
            var created = await _gameSvc.Create(game);
            if (created != null)
            {
                var routeValue = new { id = created.Id };
                return CreatedAtRoute(routeValue, created);
            }
            return BadRequest(new ResponseModel("Error", "Game not created"));
        }

        [HttpPut]
        [Authorize(Policy = "RequireManager")]
        [Route("update-game/{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] GameModel game)
        {
            var updatedGame = await _gameSvc.Update(id, game);
            if (updatedGame != null)
            {
                return Ok(updatedGame);
            }
            return NotFound(new ResponseModel("Error", "Game not found"));
        }

        [HttpDelete]
        [Authorize(Policy = "RequireManager")]
        [Route("remove-game/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            bool result = await _gameSvc.Delete(id);
            if (result) { return Ok(new ResponseModel("Success", "Game removed successfully")); }
            return NotFound(new ResponseModel("Error", "Game not found"));
        }

        [HttpPut]
        [Authorize(Policy = "RequireManager")]
        [Route("set-game-image/{id}")]
        public async Task<IActionResult> SetGameImage([FromRoute] int id, IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return BadRequest(new ResponseModel("Error", "Invalid file"));
            }

            var result = await _gameSvc.SetImage(id, ImageFile);

            if (result != null)
            {
                return Ok(new ResponseModel("Success", "Image set successfully"));
            }
            return BadRequest(new ResponseModel("Error", "Image setting failed"));

        }

        [HttpGet]
        [AllowAnonymous]
        [Route("search-game/{name}")]
        public async Task<IActionResult> Search([FromRoute] string name)
        {
            if (name.Length < 3)
            {
                return BadRequest(new ResponseModel("Error", "Minimum name length is 3 characters"));
            }
            var searched = await _gameSvc.Search(name);
            return Ok(searched);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("filter-game/{genre}")]
        public async Task<IActionResult> Filter([FromRoute] int genre)
        {
            var filtered = await _gameSvc.Filter(genre);
            return Ok(filtered);
        }

    }
}

