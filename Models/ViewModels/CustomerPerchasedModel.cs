using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class CustomerPerchasedModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public OrderItem OrderItem { get; set; }
        public OrderDetail OrderDetail { get; set; }
    }
}
