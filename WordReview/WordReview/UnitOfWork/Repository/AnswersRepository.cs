using WordReview.Model;

namespace WordReview.Repository
{
    public class AnswersRepository : BaseRepository<Answers>
    {
        public AnswersRepository(DataContext context) : base(context) { }
    }
}
