using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private DatabaseContext _dbContext;

        public OrderItemRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(OrderItem orderItem)
        {
            _dbContext.OrderItems.Update(orderItem);
        }

        public async Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderItem = await _dbContext.OrderItems.FirstOrDefaultAsync(o => o.Id == id);
            if (orderItem != null)
            {
                orderItem.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderItem.PaymentStatus = paymentStatus;
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderItem = await _dbContext.OrderItems.FirstOrDefaultAsync(o => o.Id == id);
            if (orderItem != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderItem.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderItem.PaymentIntendId = paymentIntentId;
                    orderItem.PaymentDate = DateTime.Now;
                }
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
