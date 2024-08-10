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
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        private DatabaseContext _dbContext;

        public ArtistRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Artist artist)
        {
            _dbContext.Artists.Update(artist);
        }
    }
}
