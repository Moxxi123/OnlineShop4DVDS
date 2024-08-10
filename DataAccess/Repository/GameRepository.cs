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
    public class GameRepository : Repository<Game>, IGameRepository
    {
        private DatabaseContext _dbContext;

        public GameRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Game game)
        {
            _dbContext.Games.Update(game);
        }
    }
}
