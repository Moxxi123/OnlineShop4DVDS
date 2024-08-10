using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using System.Security.Policy;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class AlbumCategoryController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public AlbumCategoryController(IUnitOfWork unitOfWork)
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
        public async Task<IActionResult> Create(AlbumCategory albumCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.AlbumCategoryRepository.Add(albumCategory);
                    await _unitOfWork.Save();
                    TempData["success"] = "Category created successfully";
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
            return View(albumCategory);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var albumCategory = await _unitOfWork.AlbumCategoryRepository.GetFirstOrDefault(c => c.Id == id);

            if (albumCategory == null)
            {
                return NotFound();
            }
            return View(albumCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AlbumCategory albumCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var categoryUpdated = await _unitOfWork.AlbumCategoryRepository.GetFirstOrDefault(c => c.Id == albumCategory.Id);
                    if (categoryUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.AlbumCategoryRepository.Update(albumCategory);
                    await _unitOfWork.Save();
                    TempData["success"] = "Category updated successfully";
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
            return View(albumCategory);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categoryList = await _unitOfWork.AlbumCategoryRepository.GetAll();
            return Json(new { data = categoryList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var categoryUpdated = await _unitOfWork.AlbumCategoryRepository.GetFirstOrDefault(c => c.Id == id);
            if (categoryUpdated == null)
            {
                return NotFound();
            }

            categoryUpdated.Status = !categoryUpdated.Status;
            _unitOfWork.AlbumCategoryRepository.Update(categoryUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var categoryDeleted = await _unitOfWork.AlbumCategoryRepository.GetFirstOrDefault(c => c.Id == id);
            if (categoryDeleted == null)
            {
                return NotFound();
            }

            var productHasCategory = await _unitOfWork.AlbumRepository.GetAll(p => p.AlbumCategoryID == categoryDeleted.Id);
            if (productHasCategory.Any())
            {
                return Json(new { success = false, message = "Category cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.AlbumCategoryRepository.Remove(categoryDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Category deleted successfully" });
        }

        #endregion
    }
}

