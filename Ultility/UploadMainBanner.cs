using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Ultility
{
    public class UploadMainBanner
    {
        private const string BaseImagePath = "Asset/Admin/banners";

        public static async Task<string> SaveImage(IFormFile formFile, IWebHostEnvironment hostEnvironment)
        {
            string fileName = $"{Guid.NewGuid()}_{formFile.FileName}";
            string finalPath = Path.Combine(hostEnvironment.WebRootPath, BaseImagePath);

            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }

            string imagePathExact = Path.Combine(finalPath, fileName);
            using (var fileStream = new FileStream(imagePathExact, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            // Return relative path
            return Path.Combine(BaseImagePath, fileName).Replace("\\", "/");
        }

        public static void DeleteImage(IWebHostEnvironment hostEnvironment, string? filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                string filePathExact = Path.Combine(hostEnvironment.WebRootPath, filePath);
                if (File.Exists(filePathExact))
                {
                    File.Delete(filePathExact);
                }
            }
        }
    }
}
