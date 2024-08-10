using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public OrderItem OrderItem { get; set; }

        public IEnumerable<OrderDetail> OrderDetails { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
