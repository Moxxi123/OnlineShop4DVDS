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
    public class GameProducerRepository : Repository<GameProducer>, IGameProducerRepository
    {
        private DatabaseContext _dbContext;

        public GameProducerRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(GameProducer gameProducer)
        {
            _dbContext.GameProducers.Update(gameProducer);
        }
    }
}
