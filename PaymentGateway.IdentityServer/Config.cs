using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace PaymentGateway.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("PaymentGatewayApi")
            };

        public static List<TestUser> TestUsers = new List<TestUser>()
        {
            new TestUser {
                SubjectId = "1",
                Username = "admin",
                Password = "admin"
            }
        };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "Apple",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("678ebc03-8fb1-407f-ac5e-ff97e8b810f5".Sha256())
                    },
                    AllowedScopes = { "PaymentGatewayApi" }
                },
                new Client
                {
                    ClientId = "Google",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("3caa7ca7-7f47-4aaa-8dec-141cc0bf7bc5".Sha256())
                    },
                    AllowedScopes = { "PaymentGatewayApi" }
                },
                new Client
                {
                    ClientId = "SwaggerApi",
                    ClientSecrets = {
                        new Secret("7da3e461-a80e-4e02-a968-e21e255c4ec6".Sha256())
                    },
                    RequireClientSecret = true,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = { "PaymentGatewayApi" },
                    AllowAccessTokensViaBrowser = true,
                    AllowedCorsOrigins = { "https://localhost:44346" }

                }
            };
    }
}
