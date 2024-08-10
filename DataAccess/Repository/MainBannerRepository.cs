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
    public class MainBannerRepository : Repository<MainBanner>, IMainBannerRepository
    {
        private DatabaseContext _dbContext;

        public MainBannerRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(MainBanner mainBanner)
        {
            _dbContext.MainBanners.Update(mainBanner);
        }
    }
}
