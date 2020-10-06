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
            //ע��IdentityServer
            services.AddIdentityServer()
               .AddDeveloperSigningCredential()
               .AddInMemoryClients(Config.Clients())
               .AddInMemoryApiResources(Config.ApiResources())
               .AddTestUsers(Config.TestUsers())
              .AddInMemoryIdentityResources(Config.GetIdentityResourceResources())
              //4.0�汾��Ҫ��ӣ���Ȼ����ʱ��ʾinvalid_scope����
              .AddInMemoryApiScopes(Config.GetApiScopes());

            //�����֤�����е�ϸ΢�����Ϊ��Ӧ���ṩ�������֤����������Դ������
            //Ĭ�������ʹ�á� Cookies����������������Դ����������������ȷҪ�� Bearer��
            //��ϸ�鿴 https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?tabs=aspnetcore2x
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddIdentityServerAuthentication(c =>
                {
                    c.Authority = "http://localhost:5000/";
                    c.RequireHttpsMetadata = false;
                    c.ApiName = "api";
                });
            //����ֱ��ӳ�䵽OAuth2.0��Χ�����������֤����
            services.AddAuthorization(c =>
            {
                c.AddPolicy("AuthorizedAccess", p => p.RequireClaim("scope", "api"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Test API V1" });

                // ��������ʹ�õ�OAuth2.0����
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
                                { "api", "��Ȩ��д����" },
                            }
                        }
                    }
                });
                // ����AuthorizeAttributea�����Ƿ���Ҫ��Ȩ����
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
            //��Ӿ�̬��Դ����
            app.UseStaticFiles();
            app.UseRouting();
            //�����֤
            app.UseAuthentication();
            app.UseAuthorization();
            //���IdentityServer
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            //api����
            app.Map("/resource-server", resourceServer =>
            {
                resourceServer.UseRouting();
                //�����֤
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
