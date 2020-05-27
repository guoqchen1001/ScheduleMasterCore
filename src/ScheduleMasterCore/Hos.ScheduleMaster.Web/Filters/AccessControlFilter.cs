using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hos.ScheduleMaster.Core.Interface;
using Hos.ScheduleMaster.Core.Models;
using Hos.ScheduleMaster.Core.Common;

namespace Hos.ScheduleMaster.Web.Filters
{
    public class AccessControlFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IAccountService _account;

        public AccessControlFilter(IHttpContextAccessor accessor, IAccountService account)
        {
            _accessor = accessor;
            _account = account;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var conn = _accessor.HttpContext.Connection;
            if (conn.RemoteIpAddress.Equals(conn.LocalIpAddress))
                return;
            
            var userName = context.HttpContext.Request.Headers["ms_auth_user"].FirstOrDefault();
            var secret = context.HttpContext.Request.Headers["ms_auth_secret"].FirstOrDefault();
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(secret))
            {
                var user = _account.GetUserbyUserName(userName);
                if (user != null && user.Status == (int)SystemUserStatus.Available)
                {
                    var se = SecurityHelper.MD5($"{userName}{user.Password}{userName}");
                    if (se == secret) return;
                }
            }
            context.Result = new UnauthorizedResult();
        }
    }
}
