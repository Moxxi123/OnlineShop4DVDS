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
    public class Album
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }
        
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
        public decimal? PromotionPrice { get; set; } //= Price - (Price * Promotion / 100),

        [ValidateNever]
        public string? MusicUrl { get; set; }//spotify album link

        [ValidateNever]
        public int? PromotionId { get; set; }

        [ForeignKey("PromotionId")]
        [ValidateNever]
        public Promotion? Promotion { get; set; }
        
        public int ArtistId { get; set; }

        [ForeignKey("ArtistId")]
        [ValidateNever]
        public Artist Artist { get; set; }

        public int AlbumProducerId { get; set; }

        [ForeignKey("AlbumProducerId")]
        [ValidateNever]
        public AlbumProducer AlbumProducer { get; set; }

        [ValidateNever]
        public string ProductType { get; set; }

        public int AlbumCategoryID { get; set; }

        [ForeignKey("AlbumCategoryID")]
        [ValidateNever]
        public AlbumCategory AlbumCategory { get; set; }

        [Required]
        public int ContentTypeId { get; set; }

        [ForeignKey("ContentTypeId")]
        [ValidateNever]
        public ContentType ContentType { get; set; }

        public bool Status { get; set; }
    }

}
