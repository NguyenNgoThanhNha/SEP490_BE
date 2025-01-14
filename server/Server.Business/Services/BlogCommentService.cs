
using Server.Business.Dtos;
using Server.Data.Entities;
using Service.Business.Services;

namespace Server.Business.Services
{
    public class BlogCommentService
    {
        private readonly AppDbContext _context;
        private readonly IAIMLService _gptService;

        public BlogCommentService(AppDbContext context,
                IAIMLService gptService)
        {
            _context = _context;
            _gptService = gptService;
        }

        public async Task<GrossDTO> CheckCommentGross(string comment)
        {
            var result = await _gptService.GetGross(comment);
            return new GrossDTO()
            {
                HasGross = result != null && result.Count > 0,
                Grosses = result
            };
        }
    }
}
