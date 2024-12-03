using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class StaffService
    {

        private readonly AppDbContext _context;
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;
        public StaffService(AppDbContext context, UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _context = context;
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<Pagination<Staff>> GetListAsync(Expression<Func<Staff, bool>> filter = null,
                                    Func<IQueryable<Staff>, IOrderedQueryable<Staff>> orderBy = null,
                                    int? pageIndex = null, // Optional parameter for pagination (page number)
                                    int? pageSize = null)
        {
            IQueryable<Staff> query = _context.Staffs;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalItemsCount = await query.CountAsync();

            if (pageIndex.HasValue && pageIndex.Value == -1)
            {
                pageSize = totalItemsCount; // Set pageSize to total count
                pageIndex = 0; // Reset pageIndex to 0
            }
            else if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            var items = await query.ToListAsync();

            return new Pagination<Staff>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = items
            };
        }

        public async Task<List<Staff>> GetStaffByCustomerIdAsync(int customerId)
        {
            var staffs = _context.Appointments
                .Include(x => x.Staff).ThenInclude(x => x.StaffInfo)
                .Where(x => x.CustomerId == customerId)
                .AsEnumerable()
                .GroupBy(x => x.StaffId)
                .Select(g => g.First())
                .Select(x => new Staff
                {
                    StaffId = x.StaffId,
                    UserId = x.Staff.UserId,
                    BranchId = x.Staff.BranchId,
                    CreatedDate = x.Staff.CreatedDate,
                    UpdatedDate = x.Staff.UpdatedDate,
                    StaffInfo = x.Staff.StaffInfo
                })
                .ToList();

            return staffs;
        }

        public async Task<Staff> GetStaffLastByCustomerIdAsync(int customerId)
        {
            var staffs = _context.Appointments
                .Include(x => x.Staff).ThenInclude(x => x.StaffInfo)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.AppointmentsTime)
                .AsEnumerable()
                .GroupBy(x => x.StaffId)
                .Select(g => g.First())
                .Select(x => new Staff
                {
                    StaffId = x.StaffId,
                    UserId = x.Staff.UserId,
                    BranchId = x.Staff.BranchId,
                    CreatedDate = x.Staff.CreatedDate,
                    UpdatedDate = x.Staff.UpdatedDate,
                    StaffInfo = x.Staff.StaffInfo
                })
                .ToList();

            return staffs.FirstOrDefault();
        }

        public async Task<ApiResponse> AssignRoleAsync(int staffId, int roleId)
        {
            try
            {
                ApiResult<Staff> result = new ApiResult<Staff>();
                result.Success = true;

                if (!await _context.Staffs.AnyAsync(x => x.StaffId == staffId))
                {
                    return ApiResponse.Error("Staff not found");
                }
                if (!await _context.UserRoles.AnyAsync(x => x.RoleId == roleId))
                {
                    return ApiResponse.Error("Role not found");
                }

                if (!result.Success)
                    return result;
                var staff = _context.Staffs.Find(staffId);
                if (staff != null)
                {
                    var user = _context.Users.Find(staff.UserId);
                    if (user != null)
                    {
                        user.RoleID = roleId;
                        user.UpdatedDate = DateTime.UtcNow;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();

                        return new ApiResponse
                        {
                            data = staff,
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }

            return ApiResponse.Error(null);
        }


        public async Task<ApiResponse> AssignBranchAsync(int staffId, int branchId)
        {
            try
            {
                ApiResult<Staff> result = new ApiResult<Staff>();
                result.Success = true;

                if (!await _context.Staffs.AnyAsync(x => x.StaffId == staffId))
                {
                    return ApiResponse.Error("Staff not found");
                }
                if (!await _context.Branchs.AnyAsync(x => x.BranchId == branchId))
                {
                    return ApiResponse.Error("Branch not found");
                }

                var staff = _context.Staffs.Find(staffId);
                if (staff != null)
                {
                    staff.BranchId = branchId;
                    staff.UpdatedDate = DateTime.UtcNow;
                    _context.Staffs.Update(staff);
                    await _context.SaveChangesAsync();

                    return ApiResponse.Succeed(staff);
                }

            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }

            return ApiResponse.Error(null);
        }

        public async Task<ApiResponse> CreateStaffAsync(CUStaffDto staffDto)
        {
            try
            {
                ApiResult<Staff> result = new ApiResult<Staff>();
                result.Success = true;
                if (staffDto == null)
                {
                    return ApiResponse.Error(null);
                }

                if (!_context.Users.Any(x => x.UserId == staffDto.UserId))
                {
                    return ApiResponse.Error("User not found");
                }

                if (!_context.Branchs.Any(x => x.BranchId == staffDto.BranchId))
                {
                    return ApiResponse.Error("Branch not found");
                }

                if (_context.Staffs.Any(x => x.UserId == staffDto.UserId))
                {
                    return ApiResponse.Error("This user already exists");
                }

                if (!result.Success)
                    return result;

                // Tạo danh mục mới
                var newStaff = new Staff
                {
                    StaffId = staffDto.StaffId,
                    UserId = staffDto.UserId,
                    BranchId = staffDto.BranchId,
                    CreatedDate = DateTime.Now,
                };

                // Thêm danh mục mới vào cơ sở dữ liệu
                await _context.Staffs.AddAsync(newStaff);
                await _context.SaveChangesAsync();

                // Trả về kết quả thành công với danh mục vừa tạo
                return ApiResponse.Succeed(newStaff);
            }
            catch (Exception ex)
            {
                // Trả về lỗi với ngoại lệ
                return ApiResponse.Error(ex.Message);
            }
        }



        public async Task<Staff?> GetStaffByIdAsync(int staffId)
        {
            try
            {
                var staff = await _context.Staffs
                    .Include(c => c.StaffInfo) // Bao gồm danh sách sản phẩm nếu cần
                    .Include(c => c.Branch) // Bao gồm danh sách sản phẩm nếu cần
                    .FirstOrDefaultAsync(c => c.StaffId == staffId);

                return staff;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (nếu có)
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;  // Trả về null nếu không tìm thấy sản phẩm hoặc có lỗi
            }
        }


        public async Task<ApiResponse> UpdateStaffAsync(CUStaffDto staffUpdateDto)
        {
            try
            {
                ApiResult<Staff> result = new ApiResult<Staff>();
                result.Success = true;
                if (staffUpdateDto == null)
                {
                    return ApiResponse.Error("Staff data is required.");
                }

                if (!_context.Users.Any(x => x.UserId == staffUpdateDto.UserId))
                {
                    return ApiResponse.Error("User not found");
                }

                if (!_context.Branchs.Any(x => x.BranchId == staffUpdateDto.BranchId))
                {
                    return ApiResponse.Error("Branch not found");
                }
                if (_context.Staffs.Any(x => x.UserId == staffUpdateDto.UserId && x.StaffId != staffUpdateDto.StaffId))
                {
                    return ApiResponse.Error("This user already exists");
                }
                if (!result.Success)
                    return result;


                var existingStaff = await _context.Staffs.FirstOrDefaultAsync(p => p.StaffId == staffUpdateDto.StaffId);
                if (existingStaff == null)
                {
                    return ApiResponse.Error("Staff not found");
                }

                existingStaff.UserId = staffUpdateDto.UserId;
                existingStaff.BranchId = staffUpdateDto.BranchId;
                existingStaff.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                var updatedStaff = await _context.Staffs.FindAsync(staffUpdateDto.StaffId);

                return ApiResponse.Succeed(updatedStaff);
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có ngoại lệ
                return ApiResponse.Error($"Error: {ex.Message}");
            }
        }


        public async Task<ApiResponse> DeleteStaffAsync(int staffId)
        {
            // Tìm danh mục trong cơ sở dữ liệu
            var staff = await _context.Staffs.FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (staff == null)
            {
                return ApiResponse.Error("Staff not found.");
            }


            _context.Staffs.Remove(staff);
            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return ApiResponse.Succeed(staff, "Staff deleted.");
        }




    }
}
