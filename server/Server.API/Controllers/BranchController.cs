﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Nest;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;

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


        [HttpGet("get-list")]
        public async Task<IActionResult> GetListBranch(
    [FromQuery] string status = "Active", // Đặt giá trị mặc định
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 5)
        {
            // Gọi service để lấy danh sách chi nhánh
            var listBranch = await _branchService.GetListBranchs(status, page, pageSize);

            // Kiểm tra kết quả
            if (listBranch == null || listBranch.data == null || !listBranch.data.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there are no branches!"
                }));
            }

            // Trả về kết quả thành công
            return Ok(ApiResult<GetAllBranchResponse>.Succeed(new GetAllBranchResponse()
            {
                message = "Get branches successfully!",
                data = listBranch.data,
                pagination = listBranch.pagination
            }));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBranches([FromQuery] string status = "Active")
        {
            // Gọi service để lấy danh sách chi nhánh
            var branches = await _branchService.GetAllBranches(status);

            // Kiểm tra kết quả
            if (branches == null || !branches.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there are no branches!",
                    data = new List<object>()
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get branches successfully!",
                data = branches
            }));
        }
        

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetBranchById(int id)
        {
            // Gọi service để lấy thông tin chi nhánh theo ID
            var branch = await _branchService.GetBranchByIdAsync(id);

            if (branch == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there are no branch!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get branch successfully!",
                data = branch
            }));
        }
        
    }
}
