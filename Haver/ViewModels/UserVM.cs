using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Haver.ViewModels
{
    public class UserVM
    {
        public string ID { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Roles")]
        public List<string> UserRoles { get; set; }

    }
}
