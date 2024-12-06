using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class UserService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public UserService(UnitOfWorks unitOfWorks, IMapper mapper, AppDbContext context)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _context = context;
        }


        public async Task<Pagination<UserModel>> GetListAsync(
     Expression<Func<User, bool>> filter = null,
     Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null,
     int? pageIndex = 0,
     int? pageSize = 10)
        {
            IQueryable<User> query = _unitOfWorks.UserRepository.GetAll()
                .Include(u => u.Staff)         
                .Include(u => u.UserRole);    



            if (filter != null)
            {
                query = query.Where(filter);
            }


            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalItemsCount = await query.CountAsync();


            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);
            }


            var users = await query.ToListAsync();


            var userModels = _mapper.Map<List<UserModel>>(users);

            return new Pagination<UserModel>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = userModels
            };
        }

        public async Task<User> GetCustomerById(int id)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.UserId == id && x.RoleID == (int)RoleConstant.RoleType.Customer && x.Status == "Active");
        }

        public async Task<UserModel> GetAccountDetail(string username)
        {
            var user = await _unitOfWorks.UserRepository
                .FindByCondition(x => x.UserName == username && x.Status == "Active")
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }


            return _mapper.Map<UserModel>(user);
        }

        public async Task<UserModel> GetUserInToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new BadRequestException("Authorization header is missing or invalid.");
            }
            // Decode the JWT token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check if the token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                throw new UnAuthorizedException("Token has expired.");
            }

            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var user = await _unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
            if (user is null)
            {
                throw new BadRequestException("Can not found User");
            }
            var userModel = _mapper.Map<UserModel>(user);
            /*if (userModel.AccountBalance != null)
            {
                var balance = SecurityUtil.Decrypt(userModel.AccountBalance);
                userModel.AccountBalance = balance;
            }*/

            return userModel;
        }
    }
}
