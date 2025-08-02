using System.IO;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using ClothesShop.Models;

namespace ClothesShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment WebHostEnvironment;
        public UploadController(IWebHostEnvironment environment) 
        {
            WebHostEnvironment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [HttpPost("image")]
        [Authorize(Roles = "Administrator")]
        [RequestSizeLimit(1_048_576_00)]
        public async Task<string> UploadImage(IFormFile image)
        {

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            string uniquePath = Path.Combine("assets", "products", uniqueFileName);
            string filePath = Path.Combine(WebHostEnvironment.WebRootPath, uniquePath);

            using var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
            await image.OpenReadStream().CopyToAsync(fs);
            await fs.FlushAsync();
            fs.Close();

            filePath = filePath.Replace(this.WebHostEnvironment.WebRootPath, "").Replace("\\", "/");

            return filePath;
        }
    }
}
