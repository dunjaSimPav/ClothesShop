using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace ClothesShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("sr-Latn-RS");
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("sr-Latn-RS");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
