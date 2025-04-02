using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchPromotionController : ControllerBase
    {
        private readonly BranchPromotionService _branchPromotionService;
        private readonly IMapper _mapper;

        public BranchPromotionController(BranchPromotionService branchPromotionService, IMapper mapper)
        {
            _branchPromotionService = branchPromotionService;
            _mapper = mapper;
        }
        
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBranchPromotion([FromQuery] int page = 1, int pageSize = 5)
        {
            var listPromotion = await _branchPromotionService.GetAllBranchPromotion(page, pageSize);
            if (listPromotion.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Hi?n t?i kh�ng c� khuy?n m�i n�o!"
                }));
            }
            return Ok(ApiResult<GetAllBranchPromotionResponse>.Succeed(new GetAllBranchPromotionResponse()
            {
                message = "L?y t?t c? ch??ng tr�nh khuy?n m�i th�nh c�ng!",
                data = listPromotion.data,
                pagination = listPromotion.pagination
            }));
        }
        
        [HttpGet("get-all-promotion-of-branch/{branchId}")]
        public async Task<IActionResult> GetAllPromotionOfBranch([FromRoute] int branchId, [FromQuery] int page = 1, int pageSize = 5)
        {
            var listPromotion = await _branchPromotionService.GetAllPromotionOfBranch(branchId, page, pageSize);
            if (listPromotion.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Hi?n t?i kh�ng c� khuy?n m�i n�o!"
                }));
            }
            return Ok(ApiResult<GetAllBranchPromotionResponse>.Succeed(new GetAllBranchPromotionResponse()
            {
                message = "L?y t?t c? ch??ng tr�nh khuy?n m�i c?a chi nh�nh th�nh c�ng!",
                data = listPromotion.data,
                pagination = listPromotion.pagination
            }));
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetByPromotionId([FromRoute] int id)
        {
            var branchPromotionModel = await _branchPromotionService.GetBranchPromotionById(id);
            if (branchPromotionModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Kh�ng t�m th?y khuy?n m�i!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y ch??ng tr�nh khuy?n m�i c?a chi nh�nh th�nh c�ng!",,
                data = _mapper.Map<BranchPromotionDTO>(branchPromotionModel)
            }));
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBranchPromotion([FromBody] BranchPromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var branchPromotionModel = await _branchPromotionService.CreateBranchPromotion(request);
            if (branchPromotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "L?i khi t?o khuy?n m�i cho chi nh�nh!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "T?o khuy?n m�i cho chi nh�nh th�nh c�ng!",
                data = _mapper.Map<BranchPromotionModel>(branchPromotionModel)
            }));
        }
        
        [Authorize(Roles = "Admin, Manager")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePromotion([FromRoute] int id, [FromBody] BranchPromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var branchPromotionExist = await _branchPromotionService.GetBranchPromotionById(id);
            if (branchPromotionExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Promotion of branch not found!"
                }));
            }

            var branchPromotionModel = await _branchPromotionService.UpdateBranchPromotion(branchPromotionExist,request);
            if (branchPromotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Kh�ng t�m th?y khuy?n m�i c?a chi nh�nh!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "C?p nh?t khuy?n m�i cho chi nh�nh th�nh c�ng!",
                data = _mapper.Map<BranchPromotionModel>(branchPromotionModel)
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

            var branchPromotionExist = await _branchPromotionService.GetBranchPromotionById(id);
            if (branchPromotionExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Kh�ng t�m th?y khuy?n m�i!"
                }));
            }

            var branchPromotionModel = await _branchPromotionService.DeleteBranchPromotion(branchPromotionExist);
            if (branchPromotionModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "L?i khi x�a khuy?n m�i!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "X�a khuy?n m�i th�nh c�ng!",
                data = _mapper.Map<BranchPromotionModel>(branchPromotionModel)
            }));
        }
    }
}
