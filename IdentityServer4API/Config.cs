using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4API
{
    public static class Config
    {
        /// <summary>
        /// 客户端
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<Client> Clients()
        {
            yield return new Client
            {
                ClientId = "test-id",
                ClientName = "Test client (Code with PKCE)",

                RedirectUris = new[] {
                    "http://localhost:5000/resource-server/swagger/oauth2-redirect.html", // Kestrel
                },

                ClientSecrets = { new Secret("test-secret".Sha256()) },
                RequireConsent = true,

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowedScopes = new[] { "api",IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile},
            };
        }
        /// <summary>
        /// api资源
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<ApiResource> ApiResources()
        {
            return new List<ApiResource>
           {
                new ApiResource("api","my api")
                {
                    Scopes ={"api"},//重要,不配置返回 invalid_scope
                }
           };
        }
        /// <summary>
        /// api范围
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
           {
                new ApiScope("api")
           };
        }
        internal static IEnumerable<IdentityResource> GetIdentityResourceResources()
        {
            return new List<IdentityResource>
           {
               new IdentityResources.OpenId(),
               new IdentityResources.Profile()
           };
        }
        /// <summary>
        /// 测试用户
        /// </summary>
        /// <returns></returns>
        internal static List<TestUser> TestUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "joebloggs",
                    Username = "admin",
                    Password = "123456"
                }
            };
        }
    }
}
