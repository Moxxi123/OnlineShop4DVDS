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
    public class RecommendProductSliderRepository : Repository<RecommendProductSlider>, IRecommendProductSliderRepository
    {
        private DatabaseContext _dbContext;

        public RecommendProductSliderRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(RecommendProductSlider recommendProductSlider)
        {
            _dbContext.RecommendProductSliders.Update(recommendProductSlider);
        }
    }
}
