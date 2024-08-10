using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class ContentTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContentTypeController(IUnitOfWork unitOfWork)
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
        public async Task<IActionResult> Create(ContentType contentType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.ContentTypeRepository.Add(contentType);
                    await _unitOfWork.Save();
                    TempData["success"] = "Content type created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Type", "Content type with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(contentType);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var contentType = await _unitOfWork.ContentTypeRepository.GetFirstOrDefault(c => c.Id == id);

            if (contentType == null)
            {
                return NotFound();
            }
            return View(contentType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ContentType contentType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var contentTypeUpdated = await _unitOfWork.ContentTypeRepository.GetFirstOrDefault(c => c.Id == contentType.Id);
                    if (contentTypeUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.ContentTypeRepository.Update(contentType);
                    await _unitOfWork.Save();
                    TempData["success"] = "Content type update successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Type", "Content type with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(contentType);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll();
            return Json(new { data = contentTypeList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var contentTypeUpdated = await _unitOfWork.ContentTypeRepository.GetFirstOrDefault(c => c.Id == id);
            if (contentTypeUpdated == null)
            {
                return NotFound();
            }

            contentTypeUpdated.Status = !contentTypeUpdated.Status;
            _unitOfWork.ContentTypeRepository.Update(contentTypeUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var contentTypeDeleted = await _unitOfWork.ContentTypeRepository.GetFirstOrDefault(c => c.Id == id);
            if (contentTypeDeleted == null)
            {
                return NotFound();
            }

            var albumCheckTask = await _unitOfWork.AlbumRepository.GetAll(c => c.ContentTypeId == contentTypeDeleted.Id);
            var movieCheckTask = await _unitOfWork.MovieRepository.GetAll(m => m.ContentTypeId == contentTypeDeleted.Id);
            var gameCheckTask = await _unitOfWork.GameRepository.GetAll(g => g.ContentTypeId == contentTypeDeleted.Id);

            if (albumCheckTask.Any() || movieCheckTask.Any() || gameCheckTask.Any())
            {
                return Json(new { success = false, message = "Content type cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.ContentTypeRepository.Remove(contentTypeDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Content type deleted successfully" });
        }

        #endregion
    }
}
