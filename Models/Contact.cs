using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The subject must be 100 characters or fewer.")]
        public string Subject { get; set; }

        [Required]
        public string Feedback { get; set; }

        [ValidateNever]
        public string? Reply { get; set; }

        public bool IsSended { get; set; }
    }
}
