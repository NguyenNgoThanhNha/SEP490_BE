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
    public class ServiceFeedbackService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ServiceFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ServiceFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.FeedbackServiceRepository
                .FindByCondition(f => f.ServiceFeedbackId == id)
                .Include(f => f.User)
                .Include(f => f.Service)
                .FirstOrDefaultAsync();

            return feedback == null ? null : _mapper.Map<ServiceFeedbackDetailDto>(feedback);
        }
    }
}
