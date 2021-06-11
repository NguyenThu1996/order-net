namespace OPS.Business.IRepository
{
    public interface IAccountRepository<T> : IGenericRepository<T> where T : class
    {
        // Task Register(LoginDto loginVm);

        bool IsLockedEventAccount(string username);
    }
}
