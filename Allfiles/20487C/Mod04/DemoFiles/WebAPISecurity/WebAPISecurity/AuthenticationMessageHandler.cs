using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebAPISecurity
{
    public class AuthenticationMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            if (request.Headers.Authorization != null &&
                request.Headers.Authorization.Scheme == "Basic")
            {
                var encodedUserPass = request.Headers.Authorization.Parameter.Trim();
                var userPass = Encoding.Default.GetString(Convert.FromBase64String(encodedUserPass));
                var parts = userPass.Split(":".ToCharArray());
                var username = parts[0];
                var password = parts[1];

                if (!AuthenticateUser(username, password))
                { 
                    // Authentication failed                    
                    response = request.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                    response.Headers.Add("WWW-Authenticate", "Basic");
                    return response;                        
                }                
            }

            response = await base.SendAsync(request, cancellationToken);

            // Add the required authentication type to the response
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                response.Headers.Add("WWW-Authenticate", "Basic");
            }

            return response;            
        }

        private bool AuthenticateUser(string username, string password)
        {
            // Use a simplified authentication check where username must be equal to password
            if (username == password)
            {
                // User is valid. Create a principal for the user
                IIdentity identity = new GenericIdentity(username);
                IPrincipal principal = new GenericPrincipal(identity, new[] { "Users", "Admins" });
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;
                return true;
            }
            return false;
        }
    }

}
