using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
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


        public async Task<Pagination<User>> GetListAsync(Expression<Func<User, bool>> filter = null,
                                    Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null,
                                    int? pageIndex = null, // Optional parameter for pagination (page number)
                                    int? pageSize = null)
        {
            IQueryable<User> query = _context.Users;
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

            return new Pagination<User>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Items = items
            };
        }

        public async Task<ApiResult<User>> GetAccountDetail(string username)
        {
            var user = await _context.Users.Select(x => new User()
            {
                UserName = x.UserName,
                FullName = x.FullName,
                Email = x.Email,
                Gender = x.Gender,
                City = x.City,
                Address = x.Address,
                BirthDate = x.BirthDate,
                PhoneNumber = x.PhoneNumber
            }).SingleOrDefaultAsync(x => x.UserName == username);
            ApiResult<User> result = new ApiResult<User>();
            result.Success = true;

            if (user == null)
            {
                result = ApiResult<User>.Error(null);
                result.ErrorMessage = "User not found";
            }

            result = ApiResult<User>.Succeed(user);
            return result;
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
