using Server.Data.UnitOfWorks;

namespace Server.Business.Service
{
    public class UserService
    {
        private readonly UnitOfWorks _unitOfWorks;

        public UserService(UnitOfWorks unitOfWorks)
        {
            this._unitOfWorks = unitOfWorks;
        }
    }
}
