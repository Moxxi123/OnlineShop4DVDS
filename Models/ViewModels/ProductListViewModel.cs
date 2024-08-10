using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ProductListViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ValidateNever]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [ValidateNever]
        public DateTime ReleaseDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000, ErrorMessage = "The price must be between 0 and 1000")]
        public decimal Price { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }

        [ValidateNever]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000, ErrorMessage = "The price must be between 0 and 1000")]
        public decimal? PromotionPrice { get; set; }

        [ValidateNever]
        public int? PromotionId { get; set; }

        [ForeignKey("PromotionId")]
        [ValidateNever]
        public Promotion? Promotion { get; set; }

        [ValidateNever]
        public string ProductType { get; set; }

        public int ContentTypeId { get; set; }

        [ForeignKey("ContentTypeId")]
        [ValidateNever]
        public ContentType ContentType { get; set; }

        [ValidateNever]
        public bool IsInSlider { get; set; }
    }
}
