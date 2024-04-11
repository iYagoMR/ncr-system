using Haver.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Haver.ViewModels
{
    [ModelMetadataType(typeof(EmployeeMetaData))]
    public class EmployeeAdminVM : EmployeeVM
    {
        public string Email { get; set; }
        public bool Active { get; set; }

        [Display(Name = "Roles")]
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}
