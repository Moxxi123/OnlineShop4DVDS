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
    public class ContentTypeRepository : Repository<ContentType>, IContentTypeRepository
    {
        private DatabaseContext _dbContext;

        public ContentTypeRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(ContentType contentType)
        {
            _dbContext.ContentTypes.Update(contentType);
        }
    }
}
