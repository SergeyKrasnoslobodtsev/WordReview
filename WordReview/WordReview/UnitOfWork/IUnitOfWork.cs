using System;

namespace WordReview.UnitOfWork
{
    public interface IUnitOfWork : IDisposable {
       void Commit();
    }
}
