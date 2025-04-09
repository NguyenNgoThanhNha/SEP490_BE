using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public class AppointmentFeedbackService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public AppointmentFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<AppointmentFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.AppointmentFeedbackRepository
                .FindByCondition(f => f.AppointmentFeedbackId == id)
                .Include(f => f.Appointment)
                .Include(f => f.User)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            var dto = _mapper.Map<AppointmentFeedbackDetailDto>(feedback);
            return dto;
        }

    }
}
