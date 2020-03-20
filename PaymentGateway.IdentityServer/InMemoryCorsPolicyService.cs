using IdentityServer4.Models;
using IdentityServer4.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.IdentityServer
{
    public class InMemoryCorsPolicyService : ICorsPolicyService
    {
        private readonly static ILogger Logger = Log.Logger;

        readonly IEnumerable<Client> clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCorsPolicyService"/> class.
        /// </summary>
        /// <param name="clients">The clients.</param>
        public InMemoryCorsPolicyService(IEnumerable<Client> clients)
        {
            this.clients = clients ?? Enumerable.Empty<Client>();
        }

        public static string GetOrigin(string url)
        {
            if (url != null && (url.StartsWith("http://") || url.StartsWith("https://")))
            {
                var idx = url.IndexOf("//", StringComparison.Ordinal);
                if (idx > 0)
                {
                    idx = url.IndexOf("/", idx + 2, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        url = url.Substring(0, idx);
                    }
                    return url;
                }
            }

            return null;
        }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var query =
                from client in clients
                from url in client.AllowedCorsOrigins
                select GetOrigin(url);

            var result = query.Contains(origin, StringComparer.OrdinalIgnoreCase);

            if (result)
            {
                Logger.Information("Client list checked and origin: {0} is allowed", origin);
            }
            else
            {
                Logger.Information("Client list checked and origin: {0} is not allowed", origin);
            }

            return Task.FromResult(result);
        }
    }
}
