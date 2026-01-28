using DataAccessLayer.GenericRepository;
using DataAccessLayer;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicsLayer.Ranks
{
    public class Rank : GenericRepository<MRank>, IRank
    {
        public Rank(ApplicationDbContext context) : base(context)
        {
        }
    }
}
