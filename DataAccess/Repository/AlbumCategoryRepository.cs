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
    public class AlbumCategoryRepository : Repository<AlbumCategory>, IAlbumCategoryRepository
    {
        private DatabaseContext _dbContext;

        public AlbumCategoryRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(AlbumCategory albumCategory)
        {
            _dbContext.AlbumCategories.Update(albumCategory);
        }
    }
}
