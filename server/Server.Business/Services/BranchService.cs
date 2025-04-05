using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class BranchService
    {
        private readonly AppDbContext _context;
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;
        public BranchService(AppDbContext context, UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _context = context;
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }        

        public async Task<Branch> GetBranchAsync(int id)
        {
            return await _context.Branchs.SingleOrDefaultAsync(x => x.BranchId == id && x.Status == "Active");
        }

        public async Task<GetAllBranchResponse> GetListBranchs(string status = "Active", int page = 1, int pageSize = 5)
        {
            // Truy vấn tất cả chi nhánh, lọc theo `status` nếu có
            var query = unitOfWorks.BranchRepository
                .FindByCondition(x => x.Status == status)
                .OrderBy(x => x.BranchId); // Sắp xếp tăng dần theo BranchId

            // Tổng số bản ghi
            var totalCount = await query.CountAsync();

            // Tổng số trang
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Áp dụng phân trang
            var pagedBranch = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Kiểm tra kết quả
            if (pagedBranch == null || !pagedBranch.Any())
            {
                return null;
            }

            // Chuyển đổi dữ liệu sang DTO
            var branchModels = _mapper.Map<List<BranchModel>>(pagedBranch);

            // Trả về kết quả
            return new GetAllBranchResponse()
            {
                data = branchModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }

        public async Task<List<BranchModel>> GetAllBranches(string status = "Active")
        {
            // Truy vấn tất cả chi nhánh, lọc theo `status` nếu có
            var query = unitOfWorks.BranchRepository
                .FindByCondition(x => x.Status == status)
                .OrderBy(x => x.BranchId); // Sắp xếp tăng dần theo BranchId

            // Lấy toàn bộ danh sách
            var branches = await query.ToListAsync();

            // Kiểm tra kết quả
            if (branches == null || !branches.Any())
            {
                return null;
            }

            // Chuyển đổi dữ liệu sang DTO
            var branchModels = _mapper.Map<List<BranchModel>>(branches);

            // Trả về danh sách chi nhánh
            return branchModels;
        }

        public async Task<BranchModel> GetBranchByIdAsync(int branchId)
        {
            
            var branch = await _context.Branchs
                .Include(b => b.ManagerBranch)  
                .Include(b => b.Company)       
                .Include(b => b.Branch_Promotion) 
                .SingleOrDefaultAsync(b => b.BranchId == branchId && b.Status == "Active");

          
            if (branch == null) return null;

           
            var branchModel = _mapper.Map<BranchModel>(branch);
            return branchModel;
        }

        public async Task<bool> CreateBranch(CreateBranchRequest request)
        {
            var branch = new Branch
            {
                BranchName = request.BranchName,
                BranchAddress = request.BranchAddress,
                BranchPhone = request.BranchPhone,
                LongAddress = request.LongAddress,
                LatAddress = request.LatAddress,
                Status = request.Status,
                ManagerId = request.ManagerId,
                CompanyId = request.CompanyId,
                District = request.District,
                WardCode = request.WardCode
            };
            // Thêm chi nhánh vào cơ sở dữ liệu
            await unitOfWorks.BranchRepository.AddAsync(branch);
            return await unitOfWorks.BranchRepository.Commit() > 0;
        }

        public async Task<bool> UpdateBranch(UpdateBranchRequest request)
        {
            var branch = await unitOfWorks.BranchRepository.GetByIdAsync(request.BranchId);
            if (branch == null) throw new BadRequestException("Chi nhánh không tồn tại!");

            // Cập nhật thông tin chi nhánh
            branch.BranchName = request.BranchName ?? branch.BranchName;
            branch.BranchAddress = request.BranchAddress ?? branch.BranchAddress;
            branch.BranchPhone = request.BranchPhone ?? branch.BranchPhone;
            branch.LongAddress = request.LongAddress ?? branch.LongAddress;
            branch.LatAddress = request.LatAddress ?? branch.LatAddress;
            branch.Status = request.Status ?? branch.Status;
            branch.ManagerId = request.ManagerId != null ? request.ManagerId : branch.ManagerId;
            branch.CompanyId = request.CompanyId != null ? request.CompanyId : branch.CompanyId;
            branch.District = request.District ?? branch.District;
            branch.WardCode = request.WardCode ?? branch.WardCode;

            // Cập nhật chi nhánh trong cơ sở dữ liệu
            unitOfWorks.BranchRepository.Update(branch);
            return await unitOfWorks.BranchRepository.Commit() > 0;
        }

        public async Task<bool> DeleteBranch(int id)
        {
            var branch = await unitOfWorks.BranchRepository.GetByIdAsync(id);
            if (branch == null) throw new BadRequestException("Chi nhánh không tồn tại!");

            // Xóa chi nhánh trong cơ sở dữ liệu
            unitOfWorks.BranchRepository.Remove(branch.BranchId);
            return await unitOfWorks.BranchRepository.Commit() > 0;
        }
    }
}
