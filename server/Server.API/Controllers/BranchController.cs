using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using AutoMapper;
using Nest;
using Server.Business.Dtos;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {


        private readonly BranchService _branchService;
        private readonly IMapper _mapper;
        

        public BranchController(BranchService branchService, IMapper mapper)
        {
            _branchService = branchService;
            _mapper = mapper;
            
        }


        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBranch(
        [FromQuery] string status,
        [FromQuery] int page = 1,
   [FromQuery] int pageSize = 5)
        {
            var listBranch = await _branchService.GetAllBranchs(status, page, pageSize);
            if (listBranch == null || listBranch.data == null || !listBranch.data.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there are no branchs!"
                }));
            }
            return Ok(ApiResult<GetAllBranchResponse>.Succeed(new GetAllBranchResponse()
            {
                message = "Get branchs successfully!",
                data = listBranch.data,
                pagination = listBranch.pagination
            }));
        }
    }
}
