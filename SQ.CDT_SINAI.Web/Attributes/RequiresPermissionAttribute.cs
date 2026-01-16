using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Web.Filters;

namespace SQ.CDT_SINAI.Web.Attributes
{
    public class RequiresPermissionAttribute : TypeFilterAttribute
    {
        public RequiresPermissionAttribute(string module, string action) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { module, action };
        }
    }
}
