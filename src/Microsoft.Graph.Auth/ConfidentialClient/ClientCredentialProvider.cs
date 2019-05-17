﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Auth
{
    using Microsoft.Identity.Client;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IAuthenticationProvider"/> implementation using MSAL.Net to acquire token by client credential flow.
    /// </summary>
    public class ClientCredentialProvider : MsalAuthenticationBase<IConfidentialClientApplication>, IAuthenticationProvider
    {
        /// <summary>
        /// Constructs a new <see cref=" ClientCredentialProvider"/>
        /// </summary>
        /// <param name="confidentialClientApplication">A <see cref="IConfidentialClientApplication"/> to pass to <see cref="ClientCredentialProvider"/> for authentication.</param>
        public ClientCredentialProvider(
            IConfidentialClientApplication confidentialClientApplication)
            : base(null)
        {
            ClientApplication = confidentialClientApplication ?? throw new AuthenticationException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.InvalidRequest,
                        Message = string.Format(ErrorConstants.Message.NullValue, "confidentialClientApplication")
                    });
        }

        /// <summary>
        /// Adds an authentication header to the incoming request by checking the application's <see cref="TokenCache"/>
        /// for an unexpired access token. If a token is not found or expired, it gets a new one.
        /// </summary>
        /// <param name="httpRequestMessage">A <see cref="HttpRequestMessage"/> to authenticate</param>
        public async Task AuthenticateRequestAsync(HttpRequestMessage httpRequestMessage)
        {
            MsalAuthenticationProviderOption msalAuthProviderOption = httpRequestMessage.GetMsalAuthProviderOption();
            int retryCount = 0;
            do
            {
                try
                {
                    AuthenticationResult authenticationResult = await ClientApplication.AcquireTokenForClient(new string[] { AuthConstants.DefaultScopeUrl })
                        .WithForceRefresh(msalAuthProviderOption.ForceRefresh)
                        .ExecuteAsync();

                    if (!string.IsNullOrEmpty(authenticationResult?.AccessToken))
                        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, authenticationResult.AccessToken);
                    break;
                }
                catch (MsalServiceException serviceException)
                {
                    if (serviceException.ErrorCode == ErrorConstants.Codes.TemporarilyUnavailable)
                    {
                        TimeSpan delay = this.GetRetryAfter(serviceException);
                        retryCount++;
                        // pause execution
                        await Task.Delay(delay);
                    }
                    else
                    {
                        throw new AuthenticationException(
                            new Error
                            {
                                Code = ErrorConstants.Codes.GeneralException,
                                Message = ErrorConstants.Message.UnexpectedMsalException
                            },
                            serviceException);
                    }
                }
                catch(Exception exception)
                {
                    throw new AuthenticationException(
                            new Error
                            {
                                Code = ErrorConstants.Codes.GeneralException,
                                Message = ErrorConstants.Message.UnexpectedException
                            },
                            exception);
                }

            } while (retryCount < MaxRetry);
        }
    }
}
