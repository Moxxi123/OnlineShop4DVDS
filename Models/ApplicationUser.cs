using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models
{
    public class ApplicationUser : IdentityUser //mở rộng class IdentityUser để thêm cột vào bảng AspNetUsers 
    {
        [Required]
        public string Name { get; set; }

        [ValidateNever]
        public string? StreetAddress { get; set; }

        [ValidateNever]
        public string? City { get; set; }

        [ValidateNever]
        public string? State { get; set; }

        [ValidateNever]
        public string? PostalCode { get; set; }

        [ValidateNever]
        [NotMapped]
        public string RoleId { get; set; }

        [ValidateNever]
        [NotMapped]
        public string Role { get; set; }


    }
}
