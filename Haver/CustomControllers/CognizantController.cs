using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Haver.CustomControllers
{
    public class CognizantController : Controller
    {
        internal string ControllerName()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }
        internal string ActionName()
        {
            return ControllerContext.RouteData.Values["action"].ToString();
        }

        internal static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "([A-Z])", " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Add the Controller and Action names to ViewData
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = SplitCamelCase(ControllerName());
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerName() + " " + ActionName();
            if (!ViewData.ContainsKey("returnURL"))
            {
                ViewData["returnURL"] = "/" + ControllerName();
            }

            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            //Add the Controller and Action names to ViewData
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = SplitCamelCase(ControllerName());
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerName() + " " + ActionName();
            if (!ViewData.ContainsKey("returnURL"))
            {
                ViewData["returnURL"] = "/" + ControllerName();
            }

            return base.OnActionExecutionAsync(context, next);
        }
    }
}
