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
    public partial class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } //id của chi tiết đơn đặt hàng

        [Required]
        public int OrderItemId { get; set; } //id của đơn đặt hàng để nối bảng order

        [ForeignKey("OrderItemId")]
        [ValidateNever]
        public OrderItem? OrderItem { get; set; }
        [Required]
        public string ProductType { get; set; }
        [Required]
        public int Count { get; set; } //số lượng hàng
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CurrentPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        [ValidateNever]
        public string? ProductName { get; set; }

        public int? AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        [ValidateNever]
        public Album? Album { get; set; }
        public int? MovieId { get; set; }
        [ForeignKey("MovieId")]
        [ValidateNever]
        public Movie? Movie { get; set; }
        public int? GameId { get; set; }
        [ForeignKey("GameId")]
        [ValidateNever]
        public Game? Game { get; set; }

    }

}
