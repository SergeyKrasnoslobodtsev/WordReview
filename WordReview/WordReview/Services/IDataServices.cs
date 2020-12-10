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
        Task<IEnumerable<ResponseCode>> GetAsyncCodes(string file);
        Task<IEnumerable<Answers>> GetAsyncLoadAnswers(string file);
        Task<IEnumerable<Answers>> GetAsyncLoadAnswers();
        IEnumerable<ResponseCode> GetCodes(string file);
        IEnumerable<Answers> GetLoadAnswers(string file);
        IEnumerable<Answers> GetLoadAnswers();
    }
}
