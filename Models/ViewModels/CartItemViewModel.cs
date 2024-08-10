using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class CartItemViewModel
    {
        public IEnumerable<CartItem> CartItems { get; set; }
        public OrderItem OrderItem { get; set; }
    }
} 
