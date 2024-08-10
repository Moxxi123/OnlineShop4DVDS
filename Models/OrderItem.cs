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
    public class OrderItem
    {
        public int Id { get; set; } //order id
        public string ApplicationUserId { get; set; } //connect user table 
        [ValidateNever]

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; } //ngày đặt hàng

        public DateTime ShippingDate { get; set; } //ngày ship hàng
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal OrderTotal { get; set; } //tổng giá trị đơn hàng
        
        public int CountTotal { get; set; } // tổng số lượng sap phẩm

        public string OrderStatus { get; set; } //trạng thái đơn hàng

        public string PaymentMethod { get; set; } // phương thức thanh toán

        [Required(ErrorMessage = "Payment status is required")]
        public string PaymentStatus { get; set; } //trạng thái thanh toán

        [ValidateNever]
        public string? TrackingNumber { get; set; } //số vận đơn nếu hàng đã được vận chuyển

        [ValidateNever]
        public string? Carrier { get; set; } //số xe vận chuyển

        public DateTime PaymentDate { get; set; } //ngày thanh toán

        public DateOnly PaymentDueDate { get; set; } //hạn thanh toán

        [ValidateNever]
        public string? SessionId { get; set; } //mã số session thực hiện thanh toán từ API

        [ValidateNever]
        public string? PaymentIntendId { get; set; } //mã số thanh toán từ API ngân hàng

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Street address is required")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [ValidateNever]
        public string? CancelRequest { get; set; }
    }
}
