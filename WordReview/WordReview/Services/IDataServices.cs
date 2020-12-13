using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordReview.Model;

namespace WordReview.Services
{
    public interface IDataServices
    {
        Task AsyncLoaderFileToData(string fileName, Int32 BufferSize = 4096);
        void LoaderFileToData(string fileName, Int32 BufferSize = 4096);
    }
}
