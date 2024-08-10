using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Models;
using Models.ViewModels;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class MovieController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MovieController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
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

            var movieProducerList = await _unitOfWork.MovieProducerRepository.GetAll(m => m.Status == false);
            ViewBag.MovieProducerList = new SelectList(movieProducerList, "Id", "ProducerName", "Id");

            var movieCategoryList = await _unitOfWork.MovieCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.MovieCategoryList = new SelectList(movieCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Movie movie, IFormFile? MainImage, IFormFile? File)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create movie with no image
                    movie.ProductType = StaticDetail.ProductType_Movie;
                    await _unitOfWork.MovieRepository.Add(movie);
                    await _unitOfWork.Save();
                    TempData["success"] = "Movie created successfully";

                    // Update ImageUrl to movie
                    if (MainImage != null)
                    {
                        string ImageUrl = await UploadImage.SaveImage(movie.Id, movie.ProductType, MainImage, _webHostEnvironment);
                        // Update image
                        movie.ImageUrl = ImageUrl;

                        _unitOfWork.MovieRepository.Update(movie);
                        await _unitOfWork.Save();
                    }

                    // Update FileUrl to movie
                    if (File != null)
                    {
                        string FileUrl = await UploadFile.SaveFile(movie.Id, movie.ProductType, File, _webHostEnvironment);
                        // Update image
                        movie.FileUrl = FileUrl;

                        _unitOfWork.MovieRepository.Update(movie);
                        await _unitOfWork.Save();
                    }
                    TempData["success"] = "Movie created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("MovieName", "Movie with the same name already exists.");
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

            var movieProducerList = await _unitOfWork.MovieProducerRepository.GetAll(m => m.Status == false);
            ViewBag.MovieProducerList = new SelectList(movieProducerList, "Id", "ProducerName", "Id");

            var movieCategoryList = await _unitOfWork.MovieCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.MovieCategoryList = new SelectList(movieCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");
            return View(movie);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == id, includeProperties: "Promotion,MovieProducer,MovieCategory,ContentType");

            var promotionList = await _unitOfWork.PromotionRepository.GetAll(p => p.Status == StaticDetail.PromotionStatus_Active);

            var selectList = promotionList.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            ViewBag.PromotionList = selectList;

            var movieProducerList = await _unitOfWork.MovieProducerRepository.GetAll(m => m.Status == false);
            ViewBag.MovieProducerList = new SelectList(movieProducerList, "Id", "ProducerName", "Id");

            var movieCategoryList = await _unitOfWork.MovieCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.MovieCategoryList = new SelectList(movieCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");

            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Movie movie, IFormFile? MainImage, IFormFile? File)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var movieUpdated = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == movie.Id);
                    if (movieUpdated == null)
                    {
                        return NotFound();
                    }

                    if (MainImage != null)
                    {
                        UploadImage.DeleteImage(_webHostEnvironment, movieUpdated?.ImageUrl);

                        movie.ImageUrl = await UploadImage.SaveImage(movieUpdated.Id, movieUpdated.ProductType, MainImage, _webHostEnvironment);
                    }

                    if (File != null)
                    {
                        UploadFile.DeleteFile(_webHostEnvironment, movieUpdated?.FileUrl);

                        movie.FileUrl = await UploadFile.SaveFile(movieUpdated.Id, movieUpdated.ProductType, File, _webHostEnvironment);
                    }

                    _unitOfWork.MovieRepository.Update(movie);
                    await _unitOfWork.Save();
                    TempData["success"] = "Movie update successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("MovieName", "Movie with the same name already exists.");
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

            var movieProducerList = await _unitOfWork.MovieProducerRepository.GetAll(m => m.Status == false);
            ViewBag.MovieProducerList = new SelectList(movieProducerList, "Id", "ProducerName", "Id");

            var movieCategoryList = await _unitOfWork.MovieCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.MovieCategoryList = new SelectList(movieCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");
            return View(movie);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movieList = await _unitOfWork.MovieRepository.GetAll(includeProperties: "Promotion,MovieCategory,ContentType");
            return Json(new { data = movieList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var movieUpdated = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieUpdated == null)
            {
                return NotFound();
            }

            movieUpdated.Status = !movieUpdated.Status;
            _unitOfWork.MovieRepository.Update(movieUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var movieDeleted = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieDeleted == null)
            {
                return NotFound();
            }

            var orderHasProduct = await _unitOfWork.OrderDetailRepository.GetAll(o => o.MovieId == movieDeleted.Id && o.ProductType == StaticDetail.ProductType_Movie);
            if (orderHasProduct.Any())
            {
                return Json(new { success = false, message = "Product cannot be deleted because there is an order assigned to it" });
            }

            _unitOfWork.MovieRepository.Remove(movieDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Movie deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFile(int id)
        {
            var movieUpdated = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieUpdated == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(movieUpdated.FileUrl))
            {
                UploadFile.DeleteFile(_webHostEnvironment, movieUpdated.FileUrl);

                movieUpdated.FileUrl = null;
            }

            _unitOfWork.MovieRepository.Update(movieUpdated);
            await _unitOfWork.Save();

            return Json(new { success = true, message = "File remove successfully" });
        }

        #endregion
    }
}
