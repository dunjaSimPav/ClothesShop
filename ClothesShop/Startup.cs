using ClothesShop.Models;
using ClothesShop.Repository;
using ClothesShop.Repository.DbSeed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClothesShop.Services;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System.Collections.Generic;
using System;

namespace ClothesShop
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<DatabaseContext>(o =>
            {
                o.UseSqlServer(Configuration["ConnectionStrings:ClothesShopConnection"]);
            });

            services.AddDbContext<IdentityContext>(o =>
            {
                o.UseSqlServer(Configuration["ConnectionStrings:ClothesShopIdentityConnection"]);
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole("Administrator"));
            });

            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddRazorPages();

            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddScoped<Cart>(x => SessionCart.GetCart(x));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<ISessionManager, SessionManager>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                app.UseExceptionHandler("/error");
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute("ArticleTypepage", "{ArticleType:int}/Page{ArticlePage:int}",
                    new { Controller = "Home", action = "Index" });

                endpoints.MapControllerRoute("OrderEditPage", "Order/Edit/{orderId:int}",
                    new { Controller = "Order", action = "Edit" });

                endpoints.MapControllerRoute("page", "Page{ArticlePage:int}",
                    new { Controller = "Home", action = "Index", ArticlePage = 1 });

                endpoints.MapControllerRoute("ArticleType", "{ArticleType}",
                    new { Controller = "Home", action = "Index", ArticlePage = 1 });

                endpoints.MapControllerRoute("pagination", "Articles/{ArticlePage:int}",
                    new { Controller = "Home", action = "Index", ArticlePage = 1 });

                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();

                endpoints.MapFallbackToPage("/admin/{*catchall}", "/Admin/Index");
            });

            SeedData.Seed(app);

            var success = SeedIdentityData.EnsurePopulated(app).Result;
            if (success)
            {
                Console.WriteLine("Data initialization succeeded!");
            }
        }
    }
}