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
    public class ContactRepository : Repository<Contact>, IContactRepository
    {
        private DatabaseContext _dbContext;

        public ContactRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Contact contact)
        {
            _dbContext.Contacts.Update(contact);
        }
    }
}
