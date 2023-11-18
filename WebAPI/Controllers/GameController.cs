using Microsoft.AspNetCore.Authorization;
using Api.Models;
using Api.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using DataAccess.Entity;

namespace Api.Controllers
{
    
    [Route("api/game")]//[controller]/[action]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameCRUDService _gameSvc;

        public GameController(IGameCRUDService gameSvc)
        {
            _gameSvc = gameSvc;
        }

        [HttpGet]
        [Route("get-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _gameSvc.GetAll());
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("get-game/{id}")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            var foundGame = await _gameSvc.Get(id);
            if (foundGame != null)
            {
                return Ok(foundGame);
            }
            return NotFound("Game not found");


        }

        [HttpPost]
        //[Authorize(Policy = "RequireManager")]
        [Route("create-game")]
        public async Task<IActionResult> Post([FromBody] GameModel game)
        {
           var createdGame = await _gameSvc.Create(game);
           var routeValue = new { id = createdGame.Id };
           return CreatedAtRoute(routeValue, createdGame);
        }

        [HttpPut]
        //[Authorize(Policy = "RequireManager")]
        [Route("update-game/{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] GameModel game)
        {
            var updatedGame = await _gameSvc.Update(id, game);
            if(updatedGame != null)
            {
                return Ok(updatedGame);
            }
            return NotFound("Game not found");
        }

        [HttpDelete]
        //[Authorize(Policy = "RequireManager")]
        [Route("remove-game/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            bool result = await _gameSvc.Delete(id);
            if (result) { return Ok("Game removed successfully"); }
            return NotFound("Game not found");
        }

        [HttpPut]
        //[Authorize]
        [Route("set-game-image/{id}")]
        public async Task<IActionResult> SetGameImage([FromRoute]int id, IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return BadRequest(new ResponseModel("Error", "Invalid file"));
            }

            var result = await _gameSvc.SetImage(id,ImageFile);

            if (result != null)
            {
                return Ok(new ResponseModel("Success", "Image set successfully"));
            }
            return BadRequest(new ResponseModel("Error", "Image setting failed"));

        }

    }
}

