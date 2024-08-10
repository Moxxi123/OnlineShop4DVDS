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
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        private DatabaseContext _dbContext;

        public MovieRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Movie movie)
        {
            _dbContext.Movies.Update(movie);
        }
    }
}
