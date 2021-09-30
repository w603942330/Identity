using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Ids4Demo
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
            services.AddIdentityServer()//Ids4服务
                  .AddDeveloperSigningCredential()//添加开发人员签名凭据
                  .AddTestUsers(Config.Users().ToList())//添加测试账号
                  .AddInMemoryIdentityResources(Config.GetIdentityResources()) //添加内存apiresource
                  .AddInMemoryApiResources(Config.GetApiResources())
                  .AddInMemoryApiScopes(Config.GetApiScopes())
                  .AddInMemoryClients(Config.GetClients());//把配置文件的Client配置资源放到内存


            //解决cookie问题
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
            //解决cookie问题
            app.UseCookiePolicy();
            app.UseStaticFiles();

            app.UseRouting();

            //启动ids4中间件
            app.UseIdentityServer();
            app.UseAuthorization();

        
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });


            #region 增加授权码模式配置
            app.UseCookiePolicy();
            app.UseAuthentication();
            //app.UseAuthorization();
            #endregion


        }

    }
}
