﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Nest;
using Server.Business.Commons.Request;
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
                    message = "Hiện tại không có chi nhánh nào!"
                }));
            }

            // Trả về kết quả thành công
            return Ok(ApiResult<GetAllBranchResponse>.Succeed(new GetAllBranchResponse()
            {
                message = "Lấy chi nhánh thành công!",
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
                    message = "Hiện tại không có chi nhánh nào!",
                    data = new List<object>()
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy chi nhánh thành công!",
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
                    message = "Hiện tại chưa có chi nhánh nào!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy chi nhánh thành công!",
                data = branch
            }));
        }
        
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
        {
            // Gọi service để tạo chi nhánh
            var branch = await _branchService.CreateBranch(request);

            if (branch == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Tạo chi nhánh thất bại!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo chi nhánh thành công!",
                data = branch
            }));
        }
        
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBranch([FromBody] UpdateBranchRequest request)
        {
            // Gọi service để cập nhật chi nhánh
            var branch = await _branchService.UpdateBranch(request);

            if (branch == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Cập nhật chi nhánh thất bại!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật chi nhánh thành công!",
                data = branch
            }));
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            // Gọi service để xóa chi nhánh
            var result = await _branchService.DeleteBranch(id);

            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Xóa chi nhánh thất bại!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Xóa chi nhánh thành công!"
            }));
        }

    }
}
