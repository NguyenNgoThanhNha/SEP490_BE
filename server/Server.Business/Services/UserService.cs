using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class UserService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public UserService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
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
