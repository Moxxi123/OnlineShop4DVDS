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
    public class MovieProducerRepository : Repository<MovieProducer>, IMovieProducerRepository
    {
        private DatabaseContext _dbContext;

        public MovieProducerRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(MovieProducer movieProducer)
        {
            _dbContext.MovieProducers.Update(movieProducer);
        }
    }
}
