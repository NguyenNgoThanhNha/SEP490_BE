﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Constants;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class AppointmentsService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public AppointmentsService(UnitOfWorks unitOfWorks, IMapper mapper, AppDbContext context)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _context = context;
        }


        public async Task<Pagination<Appointments>> GetListAsync(Expression<Func<Appointments, bool>> filter = null,
                                    Func<IQueryable<Appointments>, IOrderedQueryable<Appointments>> orderBy = null,
                                    string includeProperties = "",
                                    int? pageIndex = null, // Optional parameter for pagination (page number)
                                    int? pageSize = null)
        {
            IQueryable<Appointments> query = _context.Appointments;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
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

            return new Pagination<Appointments>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Items = items
            };
        }

        public async Task<ApiResult<Appointments>> CreateAppointmentAsync(CUAppointmentDto model)
        {
            if (!await _context.Users.AnyAsync(x => x.UserId == model.CustomerId && x.RoleID == (int)RoleConstant.RoleType.Customer && x.Status == "Active"))
            {
                return ApiResult<Appointments>.Error(null, "Customer not found");
            }

            if (!await _context.Staffs.AnyAsync(x => x.StaffId == model.StaffId))
            {
                return ApiResult<Appointments>.Error(null, "Staff not found");
            }
            if (!await _context.Services.AnyAsync(x => x.ServiceId == model.ServiceId && x.Status == "Active"))
            {
                return ApiResult<Appointments>.Error(null, "Service not found");
            }
            if (!await _context.Branchs.AnyAsync(x => x.BranchId == model.ServiceId && x.Status == "Active"))
            {
                return ApiResult<Appointments>.Error(null, "Branch not found");
            }

            var appoint = _mapper.Map<Appointments>(model);
            try
            {
                _context.Appointments.Add(appoint);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResult<Appointments>.Error(null, ex.Message.ToString());
            }
            return ApiResult<Appointments>.Succeed(appoint);
        }
    }
}
