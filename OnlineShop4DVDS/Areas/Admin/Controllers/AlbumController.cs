using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Models;
using Models.ViewModels;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class AlbumController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
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
            var promotionList = await _unitOfWork.PromotionRepository.GetAll(p => p.Status == StaticDetail.PromotionStatus_Active);

            var selectList = promotionList.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            ViewBag.PromotionList = selectList;

            var artistList = await _unitOfWork.ArtistRepository.GetAll(a => a.Status == false);
            ViewBag.ArtistList = new SelectList(artistList, "Id", "ArtistName", "Id");

            var albumProducerList = await _unitOfWork.AlbumProducerRepository.GetAll(p => p.Status == false);
            ViewBag.AlbumProducerList = new SelectList(albumProducerList, "Id", "ProducerName", "Id");

            var albumCategoryList = await _unitOfWork.AlbumCategoryRepository.GetAll(c => c.Status == false);
            ViewBag.AlbumCategoryList = new SelectList(albumCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");

            return View(new Album());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Album album, IFormFile? MainImage, IFormFile? File)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create album with no image
                    album.ProductType = StaticDetail.ProductType_Album;
                    await _unitOfWork.AlbumRepository.Add(album);
                    await _unitOfWork.Save();
                    TempData["success"] = "Album created successfully";

                    // Update ImageUrl to album
                    if (MainImage != null)
                    {
                        string ImageUrl = await UploadImage.SaveImage(album.Id, album.ProductType, MainImage, _webHostEnvironment);
                        // Update image
                        album.ImageUrl = ImageUrl;

                        _unitOfWork.AlbumRepository.Update(album);
                        await _unitOfWork.Save();
                    }

                    // Update FileUrl to album
                    if (File != null)
                    {
                        string FileUrl = await UploadFile.SaveFile(album.Id, album.ProductType, File, _webHostEnvironment);
                        // Update image
                        album.FileUrl = FileUrl;

                        _unitOfWork.AlbumRepository.Update(album);
                        await _unitOfWork.Save();
                    }

                    TempData["success"] = "Album created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("AlbumName", "Album with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            var promotionList = await _unitOfWork.PromotionRepository.GetAll(p => p.Status == StaticDetail.PromotionStatus_Active);

            var selectList = promotionList.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            ViewBag.PromotionList = selectList;

            var albumProducerList = await _unitOfWork.AlbumProducerRepository.GetAll(m => m.Status == false);
            ViewBag.AlbumProducerList = new SelectList(albumProducerList, "Id", "ProducerName", "Id");

            var artistList = await _unitOfWork.ArtistRepository.GetAll(a => a.Status == false);
            ViewBag.ArtistList = new SelectList(artistList, "Id", "ArtistName", "Id");

            var albumCategoryList = await _unitOfWork.AlbumCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.AlbumCategoryList = new SelectList(albumCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");
            return View(album);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(m => m.Id == id, includeProperties: "Promotion,AlbumProducer,AlbumCategory,ContentType,Artist");

            var promotionList = await _unitOfWork.PromotionRepository.GetAll(p => p.Status == StaticDetail.PromotionStatus_Active);

            var selectList = promotionList.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            ViewBag.PromotionList = selectList;

            var albumProducerList = await _unitOfWork.AlbumProducerRepository.GetAll(p => p.Status == false);
            ViewBag.AlbumProducerList = new SelectList(albumProducerList, "Id", "ProducerName", "Id");

            var artistList = await _unitOfWork.ArtistRepository.GetAll(a => a.Status == false);
            ViewBag.ArtistList = new SelectList(artistList, "Id", "ArtistName", "Id");

            var albumCategoryList = await _unitOfWork.AlbumCategoryRepository.GetAll(c => c.Status == false);
            ViewBag.AlbumCategoryList = new SelectList(albumCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");

            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Album album, IFormFile? MainImage, IFormFile? File)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var albumUpdated = await _unitOfWork.AlbumRepository.GetFirstOrDefault(m => m.Id == album.Id);
                    if (albumUpdated == null)
                    {
                        return NotFound();
                    }

                    if (MainImage != null)
                    {
                        UploadImage.DeleteImage(_webHostEnvironment, albumUpdated?.ImageUrl);

                        album.ImageUrl = await UploadImage.SaveImage(albumUpdated.Id, albumUpdated.ProductType, MainImage, _webHostEnvironment);
                    }

                    if(File != null)
                    {
                        UploadFile.DeleteFile(_webHostEnvironment, albumUpdated?.FileUrl);

                        album.FileUrl = await UploadFile.SaveFile(albumUpdated.Id, albumUpdated.ProductType, File, _webHostEnvironment);
                    }

                    _unitOfWork.AlbumRepository.Update(album);
                    await _unitOfWork.Save();
                    TempData["success"] = "Album update successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("AlbumName", "Album with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            var promotionList = await _unitOfWork.PromotionRepository.GetAll(p => p.Status == StaticDetail.PromotionStatus_Active);

            var selectList = promotionList.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            ViewBag.PromotionList = selectList;

            var albumProducerList = await _unitOfWork.AlbumProducerRepository.GetAll(m => m.Status == false);
            ViewBag.AlbumProducerList = new SelectList(albumProducerList, "Id", "ProducerName", "Id");

            var artistList = await _unitOfWork.ArtistRepository.GetAll(a => a.Status == false);
            ViewBag.ArtistList = new SelectList(artistList, "Id", "ArtistName", "Id");

            var albumCategoryList = await _unitOfWork.AlbumCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.AlbumCategoryList = new SelectList(albumCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");
            return View(album);
        }

        #region API CALLS
        [HttpGet] 
        public async Task<IActionResult> GetAll()
        {
            var albumList = await _unitOfWork.AlbumRepository.GetAll(includeProperties: "Promotion,AlbumCategory,ContentType,Artist");
            return Json(new { data = albumList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var albumUpdated = await _unitOfWork.AlbumRepository.GetFirstOrDefault(a => a.Id == id);
            if (albumUpdated == null)
            {
                return NotFound();
            }

            albumUpdated.Status = !albumUpdated.Status;
            _unitOfWork.AlbumRepository.Update(albumUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var albumDeleted = await _unitOfWork.AlbumRepository.GetFirstOrDefault(p => p.Id == id);
            if (albumDeleted == null)
            {
                return NotFound();
            }

            var orderHasProduct = await _unitOfWork.OrderDetailRepository.GetAll(o => o.AlbumId == albumDeleted.Id && o.ProductType == StaticDetail.ProductType_Album);
            if (orderHasProduct.Any())
            {
                return Json(new { success = false, message = "Product cannot be deleted because there is an order assigned to it" });
            }

            _unitOfWork.AlbumRepository.Remove(albumDeleted);
            await _unitOfWork.Save();

            UploadImage.DeleteImage(_webHostEnvironment, albumDeleted?.ImageUrl);

            UploadFile.DeleteFile(_webHostEnvironment, albumDeleted?.FileUrl);

            return Json(new { success = true, message = "Album deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFile(int id)
        {
            var albumUpdated = await _unitOfWork.AlbumRepository.GetFirstOrDefault(m => m.Id == id);
            if (albumUpdated == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(albumUpdated.FileUrl))
            {
                UploadFile.DeleteFile(_webHostEnvironment, albumUpdated.FileUrl);

                albumUpdated.FileUrl = null;
            }

            _unitOfWork.AlbumRepository.Update(albumUpdated);
            await _unitOfWork.Save();

            return Json(new { success = true, message = "File remove successfully" });
        }

        #endregion
    }
}
