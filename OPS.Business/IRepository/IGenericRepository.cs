using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPS.Business.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAll();

        T Get(int id);

        Task<T> GetAsync(int id);

        void Add(T entity);

        void Update(T entity);

        void Delete(int id);
    }
}
