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
    public class NewsRepository : Repository<News>, INewsRepository
    {
        private DatabaseContext _dbContext;

        public NewsRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(News news)
        {
            _dbContext.News.Update(news);
        }
    }
}
