using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class GameProducerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public GameProducerController(IUnitOfWork unitOfWork)
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
        public async Task<IActionResult> Create(GameProducer gameProducer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.GameProducerRepository.Add(gameProducer);
                    await _unitOfWork.Save();
                    TempData["success"] = "Game producer created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Producer with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(gameProducer);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var gameProducer = await _unitOfWork.GameProducerRepository.GetFirstOrDefault(m => m.Id == id);

            if (gameProducer == null)
            {
                return NotFound();
            }
            return View(gameProducer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GameProducer gameProducer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var gameProducerUpdated = await _unitOfWork.GameProducerRepository.GetFirstOrDefault(m => m.Id == gameProducer.Id);
                    if (gameProducerUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.GameProducerRepository.Update(gameProducer);
                    await _unitOfWork.Save();
                    TempData["success"] = "Game producer update successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Name", "Producer with the same name already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(gameProducer);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var gameProducerList = await _unitOfWork.GameProducerRepository.GetAll();
            return Json(new { data = gameProducerList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var gameProducerUpdated = await _unitOfWork.GameProducerRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameProducerUpdated == null)
            {
                return NotFound();
            }

            gameProducerUpdated.Status = !gameProducerUpdated.Status;
            _unitOfWork.GameProducerRepository.Update(gameProducerUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var gameProducerDeleted = await _unitOfWork.GameProducerRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameProducerDeleted == null)
            {
                return NotFound();
            }

            var gameProducerCheckTask = await _unitOfWork.GameRepository.GetAll(m => m.GameProducerId == gameProducerDeleted.Id);


            if (gameProducerCheckTask.Any())
            {
                return Json(new { success = false, message = "Game producer cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.GameProducerRepository.Remove(gameProducerDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Game producer deleted successfully" });
        }

        #endregion
    }
}
