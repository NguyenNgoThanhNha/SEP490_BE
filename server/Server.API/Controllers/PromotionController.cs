using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly PromotionService _promotionService;
        private readonly IMapper _mapper;

        public PromotionController(PromotionService promotionService, IMapper mapper)
        {
            _promotionService = promotionService;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPromotion([FromQuery] int page = 1, int pageSize = 5)
        {
            var listPromotion = await _promotionService.GetAllPromotion(page, pageSize);
            if (listPromotion.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there is no promotion!"
                }));
            }
            return Ok(ApiResult<GetAllPromotionResponse>.Succeed(new GetAllPromotionResponse()
            {
                message = "Get promotions successfully!",
                data = listPromotion.data,
                pagination = listPromotion.pagination
            }));
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetByPromotionId([FromRoute] int id)
        {
            var promotionModel = await _promotionService.GetPromotionById(id);
            if (promotionModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Promotion not found!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get promotion successfully!",
                data = _mapper.Map<PromotionDTO>(promotionModel)
            }));
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePromotion([FromForm] PromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var promotionModel = await _promotionService.CreatePromotion(request);
            if (promotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in create promotion!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create promotion successfully!",
                data = _mapper.Map<PromotionDTO>(promotionModel)
            }));
        }
        
        [Authorize(Roles = "Admin, Manager")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePromotion([FromRoute] int id, [FromForm] PromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var promotionExist = await _promotionService.GetPromotionById(id);
            if (promotionExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Promotion not found!"
                }));
            }

            var promotionModel = await _promotionService.UpdatePromotion(promotionExist,request);
            if (promotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in update promotion!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Update promotion successfully!",
                data = _mapper.Map<PromotionDTO>(promotionModel)
            }));
        }
        
        [Authorize(Roles = "Admin, Manager")]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeletePromotion([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var promotionExist = await _promotionService.GetPromotionById(id);
            if (promotionExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Promotion not found!"
                }));
            }

            var promotionModel = await _promotionService.DeletePromotion(promotionExist);
            if (promotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in delete promotion!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Delete promotion successfully!",
                data = _mapper.Map<PromotionDTO>(promotionModel)
            }));
        }
    }
}
