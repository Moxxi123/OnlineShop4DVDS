using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // lấy tất cả data or lấy data dựa trên filter đưa vào or lấy data của T , T.R
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        
        // tìm sản phẩm
        Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);

        // thêm sản phẩm
        Task Add(T item);
        // delete sản phảm
        void Remove(T item);

        // delete các items có trong T được chỉ định
        void RemoveRange(IEnumerable<T> items);
    }
}
