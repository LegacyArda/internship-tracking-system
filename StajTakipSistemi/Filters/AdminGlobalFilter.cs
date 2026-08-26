using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace StajTakipSistemi.Filters
{
    public class AdminGlobalFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Eğer kullanıcı "Yönetici" ise hiçbir kurala takılmadan geç
            if (context.HttpContext.User.IsInRole("Yönetici"))
            {
                return;
            }
        }
    }
}