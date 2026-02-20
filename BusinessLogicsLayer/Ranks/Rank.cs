using DataAccessLayer;
using DataAccessLayer.GenericRepository;
using DataTransferObject.Model;

namespace BusinessLogicsLayer.Ranks
{
    public class Rank : GenericRepository<MRank>, IRank
    {
        public Rank(ApplicationDbContext context) : base(context)
        {
        }
    }
}