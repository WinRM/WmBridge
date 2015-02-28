//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using WmBridge.Web.Model;

namespace WmBridge.Web.Filters
{
    public class PSAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public async Task AuthenticateAsync(HttpAuthenticationContext context, System.Threading.CancellationToken cancellationToken)
        {
            if (!context.Request.Headers.Contains(PSSessionManager.XPSSessionHeader))
                return;

            var session = context.Request.Headers.GetValues(PSSessionManager.XPSSessionHeader).SingleOrDefault();

            string hash; object state;
            var connection = PSSessionManager.Default.GetConnection(session, out hash, out state);

            if (connection == null)
                return;

            context.Request.Properties[PSSessionManager.PSConnectionKey] = connection;
            context.Request.Properties[PSSessionManager.PSConnectionStateKey] = state;

            ClaimsIdentity identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Hash, hash) }, "Session");
            context.Principal = new ClaimsPrincipal(identity);

            await Task.FromResult(0);
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, System.Threading.CancellationToken cancellationToken)
        {
            await Task.FromResult(0);
        }

        public bool AllowMultiple
        {
            get { return false; }
        }
    }
}
