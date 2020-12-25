
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using WordReview.Model;
using WordReview.Repository;

namespace WordReview
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        public UnitOfWork()
        {

            _context = new DataContext();
        }
        public void Commit()
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update original values from the database
                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }

            } while (saveFailed);

        }

        private  AnswersRepository answersRepository;
        public AnswersRepository AnswersRepository => answersRepository ?? (answersRepository = new AnswersRepository(_context));
        private ResponseCodeRepository responseCodeRepository;
        public ResponseCodeRepository ResponseCodeRepository => responseCodeRepository ?? (responseCodeRepository = new ResponseCodeRepository(_context));


        #region Dispose
        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
                _context.Dispose();


            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
