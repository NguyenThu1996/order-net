using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using System.Linq;

namespace OPS.Business.Repository
{
    public class AccountRepository : GenericRepository<ApplicationUser>, IAccountRepository<ApplicationUser>
    {
        public AccountRepository(OpsContext context) : base(context)
        {
        }

        public bool IsLockedEventAccount(string username)
        {
            username = username.Replace(Constants.SurveyUserName, "");
            return _context.Users.Where(u => u.UserName == username && !u.Event.IsDeleted).FirstOrDefault() == null;
        }

    }
}
