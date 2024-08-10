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
    public class GameCategoryRepository : Repository<GameCategory>, IGameCategoryRepository
    {
        private DatabaseContext _dbContext;

        public GameCategoryRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(GameCategory gameCategory)
        {
            _dbContext.GameCategories.Update(gameCategory);
        }
    }
}
