using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Models.ViewModels;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]

    public class MainBannerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MainBannerController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrCreatePermission")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MainBanner mainBanner, IFormFile? MainImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create main banner with no image
                    await _unitOfWork.MainBannerRepository.Add(mainBanner);
                    await _unitOfWork.Save();

                    // Update ImageUrl to movie
                    if (MainImage != null)
                    {
                        string ImageUrl = await UploadMainBanner.SaveImage(MainImage, _webHostEnvironment);
                        // Update image
                        mainBanner.ImageUrl = ImageUrl;

                        _unitOfWork.MainBannerRepository.Update(mainBanner);
                        await _unitOfWork.Save();
                    }

                    TempData["success"] = "Main banner created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during this operation " + ex.Message);
            }
            
            return View(mainBanner);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var mainBanner = await _unitOfWork.MainBannerRepository.GetFirstOrDefault(m => m.Id == id);

            if (mainBanner == null)
            {
                return NotFound();
            }
            return View(mainBanner);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MainBanner mainBanner, IFormFile? MainImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var mainBannerUpdated = await _unitOfWork.MainBannerRepository.GetFirstOrDefault(m => m.Id == mainBanner.Id);
                    if (mainBanner == null)
                    {
                        return NotFound();
                    }

                    if (MainImage != null)
                    {
                        UploadMainBanner.DeleteImage(_webHostEnvironment, mainBannerUpdated?.ImageUrl);

                        mainBanner.ImageUrl = await UploadMainBanner.SaveImage(MainImage, _webHostEnvironment);
                    }

                    _unitOfWork.MainBannerRepository.Update(mainBanner);
                    await _unitOfWork.Save();
                    TempData["success"] = "Main banner update successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during this operation " + ex.Message);
            }
            
            return View(mainBanner);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var mainBanner = await _unitOfWork.MainBannerRepository.GetAll();
            return Json(new { data = mainBanner });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var mainBannerUpdated = await _unitOfWork.MainBannerRepository.GetFirstOrDefault(m => m.Id == id);
            if (mainBannerUpdated == null)
            {
                return NotFound();
            }

            mainBannerUpdated.Status = !mainBannerUpdated.Status;
            _unitOfWork.MainBannerRepository.Update(mainBannerUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var mainBannerDeleted = await _unitOfWork.MainBannerRepository.GetFirstOrDefault(m => m.Id == id);
            if (mainBannerDeleted == null)
            {
                return NotFound();
            }

            _unitOfWork.MainBannerRepository.Remove(mainBannerDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Main banner deleted successfully" });
        }
        #endregion
    }
}
