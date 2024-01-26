using Api.Models;
using Api.Services;
using DataAccess.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Models;

namespace Api.Controllers
{

    [Route("api/order")]
    [ApiController]
    [EnableCors("CorsPolicy")]

    public class OrderController : ControllerBase
    {
        private readonly IGenericService<OrderModel, Order> _orderSvc;

        public OrderController(IGenericService<OrderModel, Order> orderSvc)
        {
            _orderSvc = orderSvc;
        }

        [HttpGet]
        [Route("get-all-orders")]
        [Authorize(Policy = "RequireManager")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _orderSvc.GetAll());
        }


        [HttpGet]
        [Authorize]
        [Route("get-order/{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var found = await _orderSvc.Get(id);
            if (found != null)
            {
                return Ok(found);
            }
            return NotFound(new ResponseModel("Error", "Order not found"));
        }

        [HttpPost]
        [Route("create-order")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] OrderModel model)
        {
            if (model.CommentModel != null)
            {
                model.CommentModel.Username = "anonymous";
            }

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                if (model.CommentModel != null)
                {
                    model.CommentModel.UserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    model.CommentModel.Username = HttpContext.User.Identity.Name;

                }

                model.UserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var created = await _orderSvc.Create(model);
            if (created != null)
            {
                var routeValue = new { id = created.Id };
                return CreatedAtRoute(routeValue, created);
            }
            return BadRequest(new ResponseModel("Error", "Order not created"));
        }

        [HttpPut]
        [Authorize(Policy = "RequireManager")]
        [Route("update-order/{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] OrderModel model)
        {
            Order found = await _orderSvc.Get(id);
            if (found != null)
            {
                var updated = await _orderSvc.Update(id, model);
                if (updated != null)
                {
                    return Ok(updated);
                }

            }

            return NotFound(new ResponseModel("Error", "Order not found"));
        }

        [HttpPut]
        [Authorize(Policy = "RequireManager")]
        [Route("remove-order/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            bool result = await _orderSvc.Delete(id);
            if (result) { return Ok(new ResponseModel("Success", "Order removed successfully")); }
            return NotFound(new ResponseModel("Error", "Order not found"));
        }
    }
}

