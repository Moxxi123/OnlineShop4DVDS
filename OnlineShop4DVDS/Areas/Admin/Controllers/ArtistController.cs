using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class ArtistController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public ArtistController(IUnitOfWork unitOfWork)
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
        public async Task<IActionResult> Create(Artist artist)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.ArtistRepository.Add(artist);
                    await _unitOfWork.Save();
                    TempData["success"] = "Artist created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Artist with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(artist);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var artist = await _unitOfWork.ArtistRepository.GetFirstOrDefault(c => c.Id == id);

            if (artist == null)
            {
                return NotFound();
            }
            return View(artist);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Artist artist)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var artistUpdated = await _unitOfWork.ArtistRepository.GetFirstOrDefault(c => c.Id == artist.Id);
                    if (artistUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.ArtistRepository.Update(artist);
                    await _unitOfWork.Save();
                    TempData["success"] = "Artist updated successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Artist with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(artist);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var artistList = await _unitOfWork.ArtistRepository.GetAll();
            return Json(new { data = artistList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var artistUpdated = await _unitOfWork.ArtistRepository.GetFirstOrDefault(a => a.Id == id);
            if (artistUpdated == null)
            {
                return NotFound();
            }

            artistUpdated.Status = !artistUpdated.Status;
            _unitOfWork.ArtistRepository.Update(artistUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var artistDeleted = await _unitOfWork.ArtistRepository.GetFirstOrDefault(a => a.Id == id);
            if (artistDeleted == null)
            {
                return NotFound();
            }

            var artistHasCategory = await _unitOfWork.AlbumRepository.GetAll(p => p.ArtistId == artistDeleted.Id);
            if (artistHasCategory.Any())
            {
                return Json(new { success = false, message = "Artist cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.ArtistRepository.Remove(artistDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Artist deleted successfully" });
        }

        #endregion
    }
}

