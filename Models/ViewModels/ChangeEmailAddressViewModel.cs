using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ChangeEmailAddressViewModel
    {
        [Required]
        public string UserId { get; set; }

        [EmailAddress]
        public string OldEmailAddress { get; set;}

        [Required]
        [EmailAddress]
        [Display(Name = "New Email Address")]
        public string NewEmailAddress { get; set;}

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long and contain at least one uppercase letter.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
