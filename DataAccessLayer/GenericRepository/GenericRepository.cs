using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.GenericRepository
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        protected GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<T> Get(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return entity;

        }
        public async Task<T> GetByGen<T2>(T2 val1)
        {
            var entity = await _context.Set<T>().FindAsync(val1);
            // If no entity is found, handle the case (either throw exception or return null)
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with id {val1} not found.");
            }

            return entity;
        }
        public async Task<T> GetByshort(short id)
        {
            // Use FindAsync to search for the entity by the provided id
            var entity = await _context.Set<T>().FindAsync(id);

            // If no entity is found, handle the case (either throw exception or return null)
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with id {id} not found.");
            }
            return entity;

            // return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T> GetById(int id)
        {
            // Use FindAsync to search for the entity by the provided id
            var entity = await _context.Set<T>().FindAsync(id);

            // If no entity is found, handle the case (either throw exception or return null)
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with id {id} not found.");
            }
            return entity;

            // return await _context.Set<T>().FindAsync(id);
        }
        public async Task<IEnumerable<T>> GetAll()
        {
            var result = await _context.Set<T>().ToListAsync();

            // Handle the case where result is null or empty
            if (result == null)
            {
                return Enumerable.Empty<T>();  // Return an empty collection if null
            }
            return result;
        }

        public async Task Add(T entity)
        {
            // Validate the entity (optional)
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await _context.Set<T>().AddAsync(entity);
            await SaveAsync();
        }
        public async Task<T> AddWithReturn(T entity)
        { // Validate the entity (optional)
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task Delete(T entity)
        { // Validate the entity (optional)
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Set<T>().Remove(entity);
            await SaveAsync();
        }

        public async Task Update(T entity)
        { // Validate the entity (optional)
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Entry(entity).State = EntityState.Modified;
            await SaveAsync();
        }
        public async Task<T> UpdateWithReturn(T entity)
        { // Validate the entity (optional)
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<T> Delete(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with id {id} not found.");
            }

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            return entity;
        }



    }
}
