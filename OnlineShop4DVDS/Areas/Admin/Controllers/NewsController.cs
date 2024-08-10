using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class NewsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrCreatePermission")]
        public async Task<IActionResult> Create()
        {
            return View(new News());
        }

        [HttpPost]
        public async Task<IActionResult> Create(News news, IFormFile? MainImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.NewsRepository.Add(news);
                    await _unitOfWork.Save();
                    // Update ImageUrl to news
                    if (MainImage != null)
                    {
                        string ImageUrl = await UploadPhoto.SaveImage(news.Id, news.NewsCategory, MainImage, _webHostEnvironment);
                        // Update image
                        news.ImageUrl = ImageUrl;

                        _unitOfWork.NewsRepository.Update(news);
                        await _unitOfWork.Save();
                    }

                    TempData["success"] = "News created successfully";
                    return RedirectToAction("Index", "News", new { area = "Customer", category = news.NewsCategory });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Title", "News with the same title already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(news);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var news = await _unitOfWork.NewsRepository.GetFirstOrDefault(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(News news, IFormFile? MainImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (MainImage != null)
                    {
                        var newsUpdated = await _unitOfWork.NewsRepository.GetFirstOrDefault(m => m.Id == news.Id);
                        if (newsUpdated == null)
                        {
                            return NotFound();
                        }

                        UploadPhoto.DeleteImage(_webHostEnvironment, newsUpdated?.ImageUrl);

                        news.ImageUrl = await UploadPhoto.SaveImage(newsUpdated.Id, newsUpdated.NewsCategory, MainImage, _webHostEnvironment);
                    }

                    _unitOfWork.NewsRepository.Update(news);
                    await _unitOfWork.Save();
                    TempData["success"] = "News update successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "News with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }

            return View(news);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var newsList = await _unitOfWork.NewsRepository.GetAll();
            return Json(new { data = newsList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var newsUpdated = await _unitOfWork.NewsRepository.GetFirstOrDefault(a => a.Id == id);
            if (newsUpdated == null)
            {
                return NotFound();
            }

            newsUpdated.Status = !newsUpdated.Status;
            _unitOfWork.NewsRepository.Update(newsUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var newsDeleted = await _unitOfWork.NewsRepository.GetFirstOrDefault(a => a.Id == id);
            if (newsDeleted == null)
            {
                return NotFound();
            }

            _unitOfWork.NewsRepository.Remove(newsDeleted);
            await _unitOfWork.Save();
            UploadPhoto.DeleteImage(_webHostEnvironment, newsDeleted?.ImageUrl);
            return Json(new { success = true, message = "News deleted successfully" });
        }

        #endregion
    }
}
