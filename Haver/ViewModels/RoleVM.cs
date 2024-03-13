using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Security.Claims;

namespace Haver.ViewModels
{
    public class RoleVM
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public bool Assigned { get; set; }

        
    }
}
