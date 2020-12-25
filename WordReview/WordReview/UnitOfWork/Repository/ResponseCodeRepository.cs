using WordReview.Model;

namespace WordReview.Repository
{
    public class ResponseCodeRepository : BaseRepository<ResponseCode>
    {
        public ResponseCodeRepository(DataContext context) : base(context) { }
    }
}