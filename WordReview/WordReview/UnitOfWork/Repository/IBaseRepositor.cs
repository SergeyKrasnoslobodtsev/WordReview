using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordReview.Repository
{
    interface IBaseRepository<T> where T : class
    {

        void Add(T item);
        Task AsyncAdd(T item);
        void AddRange(IEnumerable<T> items);
        Task AsyncAddRange(IEnumerable<T> items);
        void Update(T item);
        Task AsyncUpdate(T item);
        void Remove(T item);
        Task AsyncRemove(T item);
        void RemoveRange(IEnumerable<T> items);
        Task AsyncRemoveRange(IEnumerable<T> items);
        IEnumerable<T> GetAll();
        Task<List<T>> AsyncGetAll();
        IEnumerable<T> Get(Func<T, bool> predicate);
        Task<IEnumerable<T>> AsyncGet(Func<T, bool> predicate);
        T FindById(int id);
        Task<T> AsyncFindById(int id);
    }
}
