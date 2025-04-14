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
                    message = "Hiện tại không có khuyến mãi nào!"
                }));
            }
            return Ok(ApiResult<GetAllPromotionResponse>.Succeed(new GetAllPromotionResponse()
            {
                message = "Lấy chương trình khuyến mãi thành công!",
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
                    message = "Không tìm thấy khuyến mãi!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy chương trình khuyến mãi thành công!",
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
                    message = "Lỗi khi tạo khuyến mãi!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo khuyến mãi thành công!",
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
                    message = "Không tìm thấy chương trình khuyến mãi!"
                }));
            }

            var promotionModel = await _promotionService.UpdatePromotion(promotionExist,request);
            if (promotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi tạo chương trình khuyến mãi!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật chương trình khuyến mãi thành công!",
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
                    message = "Không tìm thấy khuyến mãi!"
                }));
            }

            var promotionModel = await _promotionService.DeletePromotion(promotionExist);
            if (promotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi xóa khuyến mãi!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Xóa khuyến mãi thành công!",
                data = _mapper.Map<PromotionDTO>(promotionModel)
            }));
        }
    }
}
