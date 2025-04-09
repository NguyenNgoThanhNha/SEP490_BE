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
    public class ProductFeedbackService
    {

        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ProductFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ProductFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.ProductFeedbackRepository
                .FindByCondition(f => f.ProductFeedbackId == id)
                .Include(f => f.Product)
                .Include(f => f.User)
                .FirstOrDefaultAsync();

            return feedback == null ? null : _mapper.Map<ProductFeedbackDetailDto>(feedback);
        }
    }
}
