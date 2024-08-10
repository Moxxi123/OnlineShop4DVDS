using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        void Update(OrderItem orderItem);
        Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null); //update trạng thái đơn hàng và trạng thái thanh toán

        Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId); //update sessionId thực hiện thanh toán và mã số thực hiện thanh toán
    }
}
