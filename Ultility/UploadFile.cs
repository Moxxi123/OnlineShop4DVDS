using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Ultility
{
    public class UploadFile
    {
        private const string BaseImagePath = "Asset/Admin/files/products";

        public static async Task<string> SaveFile(int id, string productType, IFormFile formFile, IWebHostEnvironment hostEnvironment)
        {
            string fileName = $"{Guid.NewGuid()}_{formFile.FileName}";
            string filePath = Path.Combine(BaseImagePath, productType, $"product-{id}");
            string finalPath = Path.Combine(hostEnvironment.WebRootPath, filePath);

            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }

            string imagePathExact = Path.Combine(finalPath, fileName);
            using (var fileStream = new FileStream(imagePathExact, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            return Path.Combine(filePath, fileName).Replace("\\", "/");
        }

        public static void DeleteFile(IWebHostEnvironment hostEnvironment, string? filePath)
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

        public static string? GetFilePath(IWebHostEnvironment hostEnvironment, string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            return Path.Combine(hostEnvironment.WebRootPath, filePath.TrimStart('/'));
        }
    }

}
