using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Branch>> GetBranchesAsync()
        {
            return await _context.Branchs.Where(x => x.Status == "Active").ToListAsync();
        }

        public async Task<Branch> GetBranchAsync(int id)
        {
            return await _context.Branchs.SingleOrDefaultAsync(x => x.BranchId == id && x.Status == "Active");
        }
    }
}
