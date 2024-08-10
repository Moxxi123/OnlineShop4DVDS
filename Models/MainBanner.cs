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
    public class MainBanner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }

        public int OrderDisplay {  get; set; }

        [ValidateNever]
        public string? Description { get; set; }

        [ValidateNever]
        public bool Status { get; set; }
    }
}
