using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatingApp.API.Helpers
{
    public class FilterModelState : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;
            if (!modelState.IsValid)
            {
                context.Result = new ContentResult
                {
                    Content = "MOdelState is not Valid",
                    StatusCode = 400
                };
                base.OnActionExecuting(context);
            }
        }
    }
}