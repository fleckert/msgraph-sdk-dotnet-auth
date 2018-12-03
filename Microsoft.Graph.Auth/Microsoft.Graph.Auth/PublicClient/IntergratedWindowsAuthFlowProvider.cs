﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Auth
{
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using System.Net.Http;
#if NET45 || NET_CORE
    // Works for tenanted & work & school accounts
    // Only available on .Net & .Net core and UWP
    public class IntergratedWindowsAuthFlowProvider : MsalAuthenticationBase, IAuthenticationProvider
    {
        private string Username;
        public IntergratedWindowsAuthFlowProvider(
           PublicClientApplication publicClientApplication,
           string[] scopes)
           : base(scopes)
        {
            ClientApplication = publicClientApplication;
        }

        public IntergratedWindowsAuthFlowProvider(
           string clientId,
           string authority,
           string[] scopes)
           : base(scopes)
        {
            ClientApplication = new PublicClientApplication(clientId, authority);
        }

        public IntergratedWindowsAuthFlowProvider(
            string clientId,
            string authority,
            TokenCache userTokenCache,
            string[] scopes)
            : base(scopes)
        {
            ClientApplication = new PublicClientApplication(clientId, authority, userTokenCache);
        }

#if !NET_CORE
        public IntergratedWindowsAuthFlowProvider(
           PublicClientApplication publicClientApplication,
           string[] scopes,
           string username)
           : base(scopes)
        {
            ClientApplication = publicClientApplication;
            Username = username;
        }
        public IntergratedWindowsAuthFlowProvider(
            string clientId,
            string authority,
            string[] scopes, 
            string username) 
            :base(scopes)
        {
            ClientApplication = new PublicClientApplication(clientId, authority);
            Username = username;
        }
        public IntergratedWindowsAuthFlowProvider(
            string clientId,
            string authority,
            TokenCache userTokenCache,
            string[] scopes,
            string username)
            :base(scopes)
        {
            ClientApplication = new PublicClientApplication(clientId, authority, userTokenCache);
            Username = username;
        }
#endif
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            await AddTokenToRequestAsync(request);
        }

        internal override async Task<AuthenticationResult> GetNewAccessTokenAsync()
        {
            AuthenticationResult authResult = null;
            IPublicClientApplication publicClientApplication = (IPublicClientApplication)ClientApplication;

            if (Username != null)
                authResult = await publicClientApplication.AcquireTokenByIntegratedWindowsAuthAsync(Scopes, Username);
            else
                authResult = await publicClientApplication.AcquireTokenByIntegratedWindowsAuthAsync(Scopes);

            return authResult;
        }
    }
#endif
}
