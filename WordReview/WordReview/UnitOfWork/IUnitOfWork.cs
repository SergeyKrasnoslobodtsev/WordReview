using System;

namespace WordReview
{
    public interface IUnitOfWork : IDisposable {
       void Commit();
    }
}
