using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models
{
    public class Game
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000, ErrorMessage = "The price must be between 0 and 1000")]
        public decimal Price { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }

        [ValidateNever]
        public string? FileUrl { get; set; }

        [ValidateNever]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000, ErrorMessage = "The price must be between 0 and 1000")]
        public decimal? PromotionPrice { get; set; }

        [ValidateNever]
        public int? PromotionId { get; set; }

        [ForeignKey("PromotionId")]
        [ValidateNever]
        public Promotion? Promotion { get; set; }

        public int GameProducerId { get; set; }

        [ForeignKey("GameProducerId")]
        [ValidateNever]
        public GameProducer GameProducer { get; set; }

        [ValidateNever]
        public string ProductType { get; set; }

        public int GameCategoryID { get; set; }

        [ForeignKey("GameCategoryID")]
        [ValidateNever]
        public GameCategory GameCategory { get; set; }

        public int ContentTypeId { get; set; }

        [ForeignKey("ContentTypeId")]
        [ValidateNever]
        public ContentType ContentType { get; set; }

        public bool Status { get; set; }

        //[ValidateNever]
        //public ICollection<Review> Reviews { get; set; }

    }

}
