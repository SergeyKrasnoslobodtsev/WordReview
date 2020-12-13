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
        public Task AsyncLoaderFileToData(string fileName, Int32 BufferSize = 4096) =>
            Task.Run(() => LoaderFileToData(fileName, BufferSize));
        public void LoaderFileToData(string fileName, Int32 BufferSize = 4096) {
            using (var fileStream = File.OpenRead(fileName))
            using (var streamReader = new StreamReader(fileStream, Encoding.Default, true, BufferSize))
            using (var uof = new UnitOfWork.UnitOfWork()) {
                String line;
                while ((line = streamReader.ReadLine()) != null) {
                    var values = line.Split('\t');
                    uof.AnswersRepository.Add(new Answers { Key = Convert.ToInt32(values[0]), Answer = values[1], OriginalAnsewr = values[1], Code = "", IsCheck = false });
                }
                uof.Commit();
            }
        }


    }
}
