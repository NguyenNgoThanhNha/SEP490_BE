using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Request;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("get-cart/{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            return Ok(await _cartService.GetCart(userId));
        }

        [HttpPost("add-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var response = await _cartService.AddToCart(request);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

       
        //public async Task<IActionResult> RemoveProductFromCart(int userId,int productBranchId)
        //{
        //    var response = await _cartService.DeleteProductFromCart(productBranchId, userId);
        //    if (response.Success)
        //    {
        //        return Ok(response);
        //    }
        //    return BadRequest(response);
        //}

        [HttpDelete("{userId}/{productBranchIds}")]
        public async Task<IActionResult> RemoveProductsFromCart(int userId, string productBranchIds)
        {
            var ids = productBranchIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            var response = await _cartService.DeleteProductsFromCart(ids, userId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }



    }
}
