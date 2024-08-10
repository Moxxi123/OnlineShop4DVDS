using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class GameCategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public GameCategoryController(IUnitOfWork unitOfWork)
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
        public async Task<IActionResult> Create(GameCategory gameCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.GameCategoryRepository.Add(gameCategory);
                    await _unitOfWork.Save();
                    TempData["success"] = "Game category created successfully";
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
            return View(gameCategory);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var GameCategory = await _unitOfWork.GameCategoryRepository.GetFirstOrDefault(m => m.Id == id);

            if (GameCategory == null)
            {
                return NotFound();
            }
            return View(GameCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GameCategory gameCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var gameCategoryUpdated = await _unitOfWork.GameCategoryRepository.GetFirstOrDefault(m => m.Id == gameCategory.Id);
                    if (gameCategoryUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.GameCategoryRepository.Update(gameCategory);
                    await _unitOfWork.Save();
                    TempData["success"] = "Game category update successfully";
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
            return View(gameCategory);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var gameCategoryList = await _unitOfWork.GameCategoryRepository.GetAll();
            return Json(new { data = gameCategoryList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var gameCategoryUpdated = await _unitOfWork.GameCategoryRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameCategoryUpdated == null)
            {
                return NotFound();
            }

            gameCategoryUpdated.Status = !gameCategoryUpdated.Status;
            _unitOfWork.GameCategoryRepository.Update(gameCategoryUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var gameCategoryDeleted = await _unitOfWork.GameCategoryRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameCategoryDeleted == null)
            {
                return NotFound();
            }

            var gameCategoryCheckTask = await _unitOfWork.GameRepository.GetAll(m => m.GameCategoryID == gameCategoryDeleted.Id);


            if (gameCategoryCheckTask.Any())
            {
                return Json(new { success = false, message = "Game category cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.GameCategoryRepository.Remove(gameCategoryDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Game category deleted successfully" });
        }

        #endregion
    }
}
