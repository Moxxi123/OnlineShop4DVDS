using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class RoleClaimViewModel
    {
        public RoleClaimViewModel()
        {
            Claims = new List<RoleClaim>(); // khởi tạo đối tượng RoleClaimViewModel bên trong controller
        }

        [ValidateNever]
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public List<RoleClaim> Claims { get; set; }

        public bool Status {  get; set; }
    }

    public class RoleClaim
    {
        public string ClaimType { get; set; }

        public bool IsSelected { get; set; }
    }
}
