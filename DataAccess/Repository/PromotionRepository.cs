using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class PromotionRepository : Repository<Promotion>, IPromotionRepository
    {
        private DatabaseContext _dbContext;

        public PromotionRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Promotion promotion)
        {
            _dbContext.Promotions.Update(promotion);
        }
    }
}
