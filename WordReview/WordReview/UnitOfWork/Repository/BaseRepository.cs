using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WordReview.Model;

namespace WordReview.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly DataContext _context;
        private readonly DbSet<T> _dbSet;
        public BaseRepository(DataContext context)
        {
            this._context = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T item)
        {
            _dbSet.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _dbSet.AddRange(items);
        }

        public Task AsyncAdd(T item)
        {
            return Task.Run(() => _dbSet.Add(item));
        }

        public Task AsyncAddRange(IEnumerable<T> items)
        {
            return Task.Run(() => _dbSet.AddRange(items));
        }

        public Task<T> AsyncFindById(int id)
        {
            return Task.Run(() => _dbSet.Find(id));
        }

        public Task<IEnumerable<T>> AsyncGet(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> AsyncGetAll()
        {
            return _dbSet.ToListAsync();
        }

        public Task AsyncRemove(T item)
        {
            throw new NotImplementedException();
        }

        public Task AsyncRemoveRange(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public Task AsyncUpdate(T item)
        {
            return Task.Run(() => _context.Entry(item).State = EntityState.Modified);
        }

        public T FindById(int id)
        {
            return _dbSet.Find(id);
        }

        public IEnumerable<T> Get(Func<T, bool> predicate)
        {
            return _dbSet.AsNoTracking().Where(predicate);
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        public void Remove(T item)
        {
            _dbSet.Remove(item);
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            _dbSet.RemoveRange(items);
        }

        public void Update(T item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }
    }
}
