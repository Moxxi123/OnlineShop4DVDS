using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AddProductToSliderRequest
    {
        public int ProductId { get; set; }
        public string ProductType { get; set; }
    }
}
