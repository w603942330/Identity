using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddAuthorization();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                  .AddCookie("Cookies")  //ʹ��Cookie��Ϊ��֤�û�����ѡ��ʽ                
                  .AddOpenIdConnect("oidc", options =>
                  {
                        options.Authority = "http://localhost:5001";  //��Ȩ��������ַ
                        options.RequireHttpsMetadata = false;  //��ʱ����https
                        options.ClientId = "app";
                        options.ClientSecret = "secret";
                        options.ResponseType = "code"; //����Authorization Code
                        options.Scope.Add("scope1"); //�����Ȩ��Դ
                        options.SaveTokens = true; //��ʾ�ѻ�ȡ��Token�浽Cookie��
                        options.GetClaimsFromUserInfoEndpoint = true;
                  });

            //���cookie����
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    SetSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    SetSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

        }
        public void SetSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                if (httpContext.Request.Scheme != "https")
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}
            //app.UseStaticFiles();

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
