using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace IdentityServer4API
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //注入IdentityServer
            services.AddIdentityServer()
               .AddDeveloperSigningCredential()
               .AddInMemoryClients(Config.Clients())
               .AddInMemoryApiResources(Config.ApiResources())
               .AddTestUsers(Config.TestUsers())
              .AddInMemoryIdentityResources(Config.GetIdentityResourceResources())
              //4.0版本需要添加，不然调用时提示invalid_scope错误
              .AddInMemoryApiScopes(Config.GetApiScopes());

            //身份验证设置有点细微差别，因为此应用提供了身份验证服务器和资源服务器
            //默认情况下使用“ Cookies”方案，并且在资源服务器控制器中明确要求“ Bearer”
            //详细查看 https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?tabs=aspnetcore2x
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddIdentityServerAuthentication(c =>
                {
                    c.Authority = "http://localhost:5000/";
                    c.RequireHttpsMetadata = false;
                    c.ApiName = "api";
                });
            //配置直接映射到OAuth2.0范围的命名身份验证策略
            services.AddAuthorization(c =>
            {
                c.AddPolicy("AuthorizedAccess", p => p.RequireClaim("scope", "api"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Test API V1" });

                // 定义正在使用的OAuth2.0方案
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("/connect/authorize", UriKind.Relative),
                            TokenUrl = new Uri("/connect/token", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                { "api", "授权读写操作" },
                            }
                        }
                    }
                });
                // 根据AuthorizeAttributea分配是否需要授权操作
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //添加静态资源引用
            app.UseStaticFiles();
            app.UseRouting();
            //身份验证
            app.UseAuthentication();
            app.UseAuthorization();
            //添加IdentityServer
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            //api配置
            app.Map("/resource-server", resourceServer =>
            {
                resourceServer.UseRouting();
                //身份验证
                resourceServer.UseAuthentication();
                resourceServer.UseAuthorization();
                //
                resourceServer.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
                //Swagger
                resourceServer.UseSwagger();
                resourceServer.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/resource-server/swagger/v1/swagger.json", "My API V1");
                    c.EnableDeepLinking();

                    // Additional OAuth settings (See https://github.com/swagger-api/swagger-ui/blob/v3.10.0/docs/usage/oauth2.md)
                    c.OAuthClientId("test-id");
                    c.OAuthClientSecret("test-secret");
                    c.OAuthAppName("test-app");
                    c.OAuthScopeSeparator(" ");
                    c.OAuthUsePkce();
                });
            });
        }
    }
}
