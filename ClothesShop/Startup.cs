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
using System;
using Stripe;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace ClothesShop
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public IWebHostEnvironment _env { get; set; }

        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            Configuration = config;
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            string stripeKey = Configuration["Stripe:SecretKey"] ?? throw new Exception("Stripe SecretKey is not configured!");
            StripeConfiguration.ApiKey = stripeKey;

            services.AddLocalization();

            services.AddSignalR(o => o.MaximumReceiveMessageSize = 104857600); // 100 MB

            services.AddHttpClient();

            services.AddServerSideBlazor();

            services.Configure<FormOptions>(o => { o.MultipartBodyLengthLimit = 104857600; });

            services.AddSingleton(x => new Localizer(x.GetRequiredService<IStringLocalizerFactory>(), typeof(Resource)));

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

            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Path.Combine(_env.ContentRootPath, "keys")))
                .SetApplicationName("ClothesShop")
                .UseCryptographicAlgorithms(new Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ValidationAlgorithm.HMACSHA256
                });

            services.AddAuthentication();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole("Administrator"));
            });

            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IArticleGroupRepository, ArticleGroupRepository>();
            services.AddScoped<IArticleGroupItemsRepository, ArticleGroupItemsRepository>();
            services.AddRazorPages();

            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddScoped<Cart>(x => SessionCart.GetCart(x));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<ISessionManager, SessionManager>();

            services.AddServerSideBlazor();
        }
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

                endpoints.MapControllerRoute("Index", "", new { Controller = "Home", action = "Index" });

                endpoints.MapControllerRoute("OrderEditPage", "Order/Edit/{orderId:int}",
                    new { Controller = "Order", action = "Edit" });

                endpoints.MapControllerRoute("ArticleGroup", "{ArticleGroup}",
                    new { Controller = "Home", action = "Index", ArticlePage = 1 });

                endpoints.MapControllerRoute("pagination", "Articles/{ArticlePage:int}",
                    new { Controller = "Home", action = "Index", ArticlePage = 1 });

                endpoints.MapDefaultControllerRoute();

                endpoints.MapControllers();

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
