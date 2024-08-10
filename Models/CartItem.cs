using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductType { get; set; }

        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        public decimal Total { get; set; }

        public decimal Price { get; set; }

        public decimal? PromotionPrice { get; set; }

        [ValidateNever]
        public Album? Album { get; set; }

        [ValidateNever]
        public Movie? Movie { get; set; }
        [ValidateNever]
        public Game? Game { get; set; }


        public CartItem()
        {


        }




    }
}
