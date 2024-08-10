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
    public class AlbumProducerRepository : Repository<AlbumProducer>, IAlbumProducerRepository
    {
        private DatabaseContext _dbContext;

        public AlbumProducerRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(AlbumProducer albumProducer)
        {
            _dbContext.AlbumProducers.Update(albumProducer);
        }
    }
}
