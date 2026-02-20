namespace DataAccessLayer.GenericRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> Get(int id);

        Task<T> GetByGen<T2>(T2 val1);

        Task<T> GetByshort(short id);

        Task<T> GetById(int id);

        Task<IEnumerable<T>> GetAll();

        Task Add(T entity);

        Task<T> AddWithReturn(T entity);

        Task Delete(T entity);

        Task<T> Delete(int id);

        Task Update(T entity);

        Task<T> UpdateWithReturn(T entity);
    }
}