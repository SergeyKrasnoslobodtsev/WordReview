using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordReview.Model;

namespace WordReview.Services
{
    public class DataServices : IDataServices
    {
        string Trimmer(string str)
        {
            char[] charsToTrim = { '*', ' ', '"' };
            return str.Trim(charsToTrim);
        }

        public Task<IEnumerable<ResponseCode>> GetAsyncCodes(string file) =>
            Task.Run(() => GetCodes(file));

        

        public IEnumerable<ResponseCode> GetCodes(string file)
        {
            var lines = File.ReadAllLines(file, Encoding.Default);
            var items = new List<ResponseCode>();
            foreach (var line in lines)
            {
                var values = Trimmer(line).Split('\t');
                items.Add(new ResponseCode { Id = Convert.ToInt32(values[0]), Word = values[1] });
            }
            return items;
        }
       

        public Task<IEnumerable<Answers>> GetAsyncLoadAnswers(string file) =>
            Task.Run(() => GetLoadAnswers(file));

        public IEnumerable<Answers> GetLoadAnswers(string file)
        {
            GetAnswers(file);
            
            var lines = File.ReadAllLines("Respondents.txt", Encoding.Default);
            var items = new List<Answers>();
            foreach (var line in lines)
            {
                var values = Trimmer(line).Split('\t');
                items.Add(new Answers { Id = Convert.ToInt32(values[0]), Answer = values[1], Code = values[2], IsCheck = Convert.ToBoolean(values[3]) });
            }
            return items;
        }
        public Task<IEnumerable<Answers>> GetAsyncLoadAnswers() =>
            Task.Run(() => GetLoadAnswers());

        public IEnumerable<Answers> GetLoadAnswers()
        {
            var lines = File.ReadAllLines("Respondents.txt", Encoding.Default);
            var items = new List<Answers>();
            foreach (var line in lines)
            {
                var values = Trimmer(line).Split('\t');
                items.Add(new Answers { Id = Convert.ToInt32(values[0]), Answer = values[1], Code = values[2], IsCheck = Convert.ToBoolean(values[3]) });
            }
            return items;
        }
        public void GetAnswers(string file)
        {
            var lines = File.ReadAllLines(file, Encoding.Default);
            var items = new List<Answers>();
            int i = 0;
            foreach (var line in lines)
            {
                var values = Trimmer(line).Split('\t');
                items.Add(new Answers { Id = i++, Answer = values[0], Code = "", IsCheck = false });
            }
            SaveItemsToFile(items);
        }

        private void SaveItemsToFile(IEnumerable<Answers> answers)
        {
            using (var uof = new UnitOfWork.UnitOfWork())
            {
                uof.AnswersRepository.AddRange(answers);
                uof.Commit();
            }
        }
    }
}
