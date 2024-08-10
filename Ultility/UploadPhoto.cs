using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Ultility
{
    public class UploadPhoto
    {
        private const string BaseImagePath = "Asset/Admin/images/news";

        public static async Task<string> SaveImage(int id, string newsCategory, IFormFile formFile, IWebHostEnvironment hostEnvironment)
        {
            string imageName = $"{Guid.NewGuid()}_{formFile.FileName}";
            string imagePath = Path.Combine(BaseImagePath, newsCategory, $"news-{id}");
            string finalPath = Path.Combine(hostEnvironment.WebRootPath, imagePath);

            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }

            string imagePathExact = Path.Combine(finalPath, imageName);
            using (var fileStream = new FileStream(imagePathExact, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            return Path.Combine(imagePath, imageName).Replace("\\", "/");
        }

        public static void DeleteImage(IWebHostEnvironment hostEnvironment, string imagePath)
        {
            string imagePathExact = Path.Combine(hostEnvironment.WebRootPath, imagePath);
            if (File.Exists(imagePathExact))
            {
                File.Delete(imagePathExact);
            }
        }
    }

}
