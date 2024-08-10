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
    public class AlbumProducerController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public AlbumProducerController(IUnitOfWork unitOfWork)
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
            return View(new AlbumProducer());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AlbumProducer albumProducer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.AlbumProducerRepository.Add(albumProducer);
                    await _unitOfWork.Save();
                    TempData["success"] = "Album Producer created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Album Producer with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(albumProducer);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var albumProducer = await _unitOfWork.AlbumProducerRepository.GetFirstOrDefault(c => c.Id == id);

            if (albumProducer == null)
            {
                return NotFound();
            }
            return View(albumProducer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AlbumProducer albumProducer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var albumProducerUpdated = await _unitOfWork.AlbumProducerRepository.GetFirstOrDefault(c => c.Id == albumProducer.Id);
                    if (albumProducerUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.AlbumProducerRepository.Update(albumProducer);
                    await _unitOfWork.Save();
                    TempData["success"] = "Album Producer updated successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Album Producer with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(albumProducer);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var albumProducerList = await _unitOfWork.AlbumProducerRepository.GetAll();
            return Json(new { data = albumProducerList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var albumProducerUpdated = await _unitOfWork.AlbumProducerRepository.GetFirstOrDefault(a => a.Id == id);
            if (albumProducerUpdated == null)
            {
                return NotFound();
            }

            albumProducerUpdated.Status = !albumProducerUpdated.Status;
            _unitOfWork.AlbumProducerRepository.Update(albumProducerUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var albumProducerDeleted = await _unitOfWork.AlbumProducerRepository.GetFirstOrDefault(a => a.Id == id);
            if (albumProducerDeleted == null)
            {
                return NotFound();
            }

            var albumProducerHasCategory = await _unitOfWork.AlbumRepository.GetAll(p => p.AlbumProducerId == albumProducerDeleted.Id);
            if (albumProducerHasCategory.Any())
            {
                return Json(new { success = false, message = "Album Producer cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.AlbumProducerRepository.Remove(albumProducerDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Album producer deleted successfully" });
        }

        #endregion
    }
}

