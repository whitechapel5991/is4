// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new ApiResource[]
            {
            new ApiResource
            {
                Name = "api",
                DisplayName = "API #1",
                Description = "Allow the application to access API #1 on your behalf",
                Scopes = new List<string> {"api.read", "api.write"},
                ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                UserClaims = new List<string> {"role"}
            }
        };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new ApiScope[]
            {
                new ApiScope("api.read", "Read Access to API #1"),
                new ApiScope("api.write", "Write Access to API #1")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new Client[]
            {
                new Client
                {
                    ClientId = "client",
                    ClientName = "Example client application using client credentials",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret> {new Secret("secret".Sha256())}, // change me!
                    AllowedScopes = new List<string> {"api.read"}
                },
                new Client
                {
                    ClientId = "spa",
                    ClientName = "Example Client Application Single Page Javascript App",
                    //ClientSecrets = new List<Secret> {new Secret("SuperSecretPassword".Sha256())}, // change me!
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,

                    // no consent page
                    RequireConsent = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    // where to redirect to after login
                    RedirectUris = {"http://localhost:5005/callback.html"},
                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5005/index.html" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "role",
                        "api",
                        "api.read",
                        "api.write",
                    },

                    //RequirePkce = true,
                    //AllowPlainTextPkce = false
                }
            };
        }

        internal static List<TestUser> GetTestUsers()
        {
            return new List<TestUser>
            {
                new TestUser { SubjectId = "65886359-073C-434B-AD2D-A3932222DABE", Username = "alice", Password = "alice123L$",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.Email, "alice@email.com"),
                        new Claim(JwtClaimTypes.Role, "student")
                    }
                },
                new TestUser { SubjectId = "12BE8632259-073C-434B-AD2D-A3932222DABE", Username = "bob", Password = "bob123L$",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.Role, "student")
                    }
                },
                new TestUser {
                    SubjectId = "5BE86359-073C-434B-AD2D-A3932222DABE",
                    Username = "scott",
                    Password = "password123L$",
                    Claims = new List<Claim> {
                        new Claim(JwtClaimTypes.Name, "Scott Smith"),
                        new Claim(JwtClaimTypes.Email, "scott@scottbrady91.com"),
                        new Claim(JwtClaimTypes.Role, "teacher")
                    } 
                }
            };
        }
    }
}
