using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;

namespace backend
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var currentContext = httpContext.AuthenticateAsync().Result;

            return currentContext.Succeeded && currentContext.Principal.IsInRole(Role.Admin);
        }
    }
}
