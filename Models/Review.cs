using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
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
        public string? ApplicationUserId { get; set; } //connect user table 
        [ValidateNever]

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        public string? ProductType { get; set; }

        public string? ProductName { get; set; }
        public string Name { get; set; }
        [ValidateNever]
        public int? StarRate { get; set; }

        [ValidateNever]
        public string? Content { get; set; }

        public DateTime CreateDate { get; set; }

        [ValidateNever]
        public int? OrderItemId { get; set; }

        [ValidateNever]
        public OrderItem? OrderItem { get; set; }


    }
}
