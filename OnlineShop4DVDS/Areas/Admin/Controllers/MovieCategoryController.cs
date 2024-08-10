using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class MovieCategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MovieCategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
        public async Task<IActionResult> Create(MovieCategory movieCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _unitOfWork.MovieCategoryRepository.Add(movieCategory);
                    await _unitOfWork.Save();
                    TempData["success"] = "Movie category created successfully";

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Category with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(movieCategory);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var movieCategory = await _unitOfWork.MovieCategoryRepository.GetFirstOrDefault(m => m.Id == id);

            if (movieCategory == null)
            {
                return NotFound();
            }
            return View(movieCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MovieCategory movieCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var movieCategoryUpdated = await _unitOfWork.MovieCategoryRepository.GetFirstOrDefault(m => m.Id == movieCategory.Id);
                    if (movieCategoryUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.MovieCategoryRepository.Update(movieCategory);
                    await _unitOfWork.Save();
                    TempData["success"] = "Movie category update successfully";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Category with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(movieCategory);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movieCategoryList = await _unitOfWork.MovieCategoryRepository.GetAll();
            return Json(new { data = movieCategoryList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var movieCategoryUpdated = await _unitOfWork.MovieCategoryRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieCategoryUpdated == null)
            {
                return NotFound();
            }

            movieCategoryUpdated.Status = !movieCategoryUpdated.Status;
            _unitOfWork.MovieCategoryRepository.Update(movieCategoryUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var movieCategoryDeleted = await _unitOfWork.MovieCategoryRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieCategoryDeleted == null)
            {
                return NotFound();
            }

            var movieCategoryCheckTask = await _unitOfWork.MovieRepository.GetAll(m => m.MovieCategoryID == movieCategoryDeleted.Id);


            if (movieCategoryCheckTask.Any())
            {
                return Json(new { success = false, message = "Movie category cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.MovieCategoryRepository.Remove(movieCategoryDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Movie category deleted successfully" });
        }

        #endregion
    }
}
