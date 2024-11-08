using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> Get([FromQuery] int page = 1)
        {
            var products = await _productService.GetAllProduct(page);
            return Ok(ApiResult<GetAllProductPaginationResponse>.Succeed(new GetAllProductPaginationResponse()
            {
                data = products.data,
                pagination = products.pagination
            }));
        }      
    }
}
