using Microsoft.AspNetCore.Authorization;

namespace WebApiShop
{
    /// <summary>
    /// Custom authorization attribute that restricts access to admin users only.
    /// Checks the "isAdmin" claim in the JWT token.
    /// </summary>
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute() : base("AdminOnly")
        {
        }
    }
}
