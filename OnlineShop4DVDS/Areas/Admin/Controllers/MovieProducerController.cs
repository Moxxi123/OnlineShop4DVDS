using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class MovieProducerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MovieProducerController(IUnitOfWork unitOfWork)
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
            return View(new MovieProducer());
        }

        [HttpPost]
        public async Task<IActionResult> Create(MovieProducer movieProducer)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _unitOfWork.MovieProducerRepository.Add(movieProducer);
                    await _unitOfWork.Save();
                    TempData["success"] = "Movie producer created successfully";

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
            return View(movieProducer);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var movieProducer = await _unitOfWork.MovieProducerRepository.GetFirstOrDefault(m => m.Id == id);

            if (movieProducer == null)
            {
                return NotFound();
            }
            return View(movieProducer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MovieProducer movieProducer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var movieProducerUpdated = await _unitOfWork.MovieProducerRepository.GetFirstOrDefault(m => m.Id == movieProducer.Id);
                    if (movieProducerUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.MovieProducerRepository.Update(movieProducer);
                    await _unitOfWork.Save();
                    TempData["success"] = "Movie producer update successfully";
                }
                return RedirectToAction("Index");
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
            return View(movieProducer);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movieProducerList = await _unitOfWork.MovieProducerRepository.GetAll();
            return Json(new { data = movieProducerList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var movieProducerUpdated = await _unitOfWork.MovieProducerRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieProducerUpdated == null)
            {
                return NotFound();
            }

            movieProducerUpdated.Status = !movieProducerUpdated.Status;
            _unitOfWork.MovieProducerRepository.Update(movieProducerUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var movieProducerDeleted = await _unitOfWork.MovieProducerRepository.GetFirstOrDefault(m => m.Id == id);
            if (movieProducerDeleted == null)
            {
                return NotFound();
            }

            var movieProducerCheckTask = await _unitOfWork.MovieRepository.GetAll(m => m.MovieProducerId == movieProducerDeleted.Id);


            if (movieProducerCheckTask.Any())
            {
                return Json(new { success = false, message = "Movie producer cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.MovieProducerRepository.Remove(movieProducerDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Movie producer deleted successfully" });
        }

        #endregion
    }
}
