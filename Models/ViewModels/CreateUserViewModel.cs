using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [ValidateNever]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [ValidateNever]
        [Display(Name = "Street Address")]
        public string? StreetAddress { get; set; }

        [ValidateNever]
        public string? City { get; set; }

        [ValidateNever]
        public string? State { get; set; }

        [ValidateNever]
        [Display(Name = "PostalCode")]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long and contain at least one uppercase letter.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string RoleId { get; set; }

        [ValidateNever]
        public string Role { get; set; }


    }
}
