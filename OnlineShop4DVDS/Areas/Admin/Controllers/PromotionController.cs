using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class PromotionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PromotionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrCreatePermission")]
        public IActionResult Create(int? id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (promotion.StartDate >= promotion.EndDate)
                    {
                        ModelState.AddModelError("", "Start date must be earlier than the end date");
                        return View(promotion);
                    }

                    if (promotion.StartDate > DateTime.Now)
                    {
                        promotion.Status = StaticDetail.PromotionStatus_Deactive;
                    }
                    else if (promotion.EndDate < DateTime.Now)
                    {
                        promotion.Status = StaticDetail.PromotionStatus_Expired;
                    }
                    else
                    {
                        promotion.Status = StaticDetail.PromotionStatus_Active;
                    }

                    await _unitOfWork.PromotionRepository.Add(promotion);
                    await _unitOfWork.Save();
                    TempData["success"] = "Promotion created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Title", "News with the same title already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(promotion);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetFirstOrDefault(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound();
            }
            return View(promotion);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Promotion promotion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (promotion.StartDate >= promotion.EndDate)
                    {
                        ModelState.AddModelError("", "Start date must be earlier than the end date");
                        return View(promotion);
                    }

                    if (promotion.StartDate > DateTime.Now)
                    {
                        promotion.Status = StaticDetail.PromotionStatus_Deactive;
                    }
                    else if (promotion.EndDate < DateTime.Now)
                    {
                        promotion.Status = StaticDetail.PromotionStatus_Expired;
                    }
                    else
                    {
                        promotion.Status = StaticDetail.PromotionStatus_Active;
                    }

                    var promotionUpdated = await _unitOfWork.PromotionRepository.GetFirstOrDefault(p => p.Id == promotion.Id);
                    if (promotionUpdated == null)
                    {
                        return NotFound();
                    }
                    _unitOfWork.PromotionRepository.Update(promotion);
                    await _unitOfWork.Save();
                    TempData["success"] = "Promotion updated successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("Description", "Promotion with the same description already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred during this operation");
                }
            }
            return View(promotion);
        }


        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var promotionList = await _unitOfWork.PromotionRepository.GetAll();
            foreach (var promotion in promotionList)
            {
                if (promotion.EndDate < DateTime.Now)
                {
                    promotion.Status = StaticDetail.PromotionStatus_Expired;

                    var albumPromotionUpdated = await _unitOfWork.AlbumRepository.GetAll(a => a.PromotionId == promotion.Id);
                    foreach (var album in albumPromotionUpdated)
                    {
                        album.PromotionId = null;
                    }

                    var moviePromotionUpdated = await _unitOfWork.MovieRepository.GetAll(m => m.PromotionId == promotion.Id);
                    foreach (var movie in moviePromotionUpdated)
                    {
                        movie.PromotionId = null;
                    }

                    var gamePromotionUpdated = await _unitOfWork.GameRepository.GetAll(g => g.PromotionId == promotion.Id);
                    foreach (var game in gamePromotionUpdated)
                    {
                        game.PromotionId = null;
                    }
                }
            }
            await _unitOfWork.Save();

            return Json(new { data = promotionList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var promotionUpdated = await _unitOfWork.PromotionRepository.GetFirstOrDefault(p => p.Id == id);
            if (promotionUpdated == null)
            {
                return NotFound();
            }

            bool isDeactivated = false;

            if (promotionUpdated.Status == StaticDetail.PromotionStatus_Active)
            {
                promotionUpdated.Status = StaticDetail.PromotionStatus_Deactive;
                isDeactivated = true;
            }
            else if (promotionUpdated.Status == StaticDetail.PromotionStatus_Deactive)
            {
                promotionUpdated.Status = StaticDetail.PromotionStatus_Active;
                isDeactivated = false;
            }
            else
            {
                return Json(new { success = false, message = "Status change not allowed for expired promotions" });
            }

            if (isDeactivated)
            {
                var albumPromotionUpdated = await _unitOfWork.AlbumRepository.GetAll(a => a.PromotionId == promotionUpdated.Id);
                foreach (var album in albumPromotionUpdated)
                {
                    album.PromotionId = null;
                }

                var moviePromotionUpdated = await _unitOfWork.MovieRepository.GetAll(m => m.PromotionId == promotionUpdated.Id);
                foreach (var movie in moviePromotionUpdated)
                {
                    movie.PromotionId = null;
                }

                var gamePromotionUpdated = await _unitOfWork.GameRepository.GetAll(g => g.PromotionId == promotionUpdated.Id);
                foreach (var game in gamePromotionUpdated)
                {
                    game.PromotionId = null;
                }
            }

            _unitOfWork.PromotionRepository.Update(promotionUpdated);
            await _unitOfWork.Save();

            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var promotionDeleted = await _unitOfWork.PromotionRepository.GetFirstOrDefault(m => m.Id == id);
            if (promotionDeleted == null)
            {
                return NotFound();
            }

            var albumCheckTask = await _unitOfWork.AlbumRepository.GetAll(c => c.PromotionId == promotionDeleted.Id);
            var movieCheckTask = await _unitOfWork.MovieRepository.GetAll(m => m.PromotionId == promotionDeleted.Id);
            var gameCheckTask = await _unitOfWork.GameRepository.GetAll(g => g.ContentTypeId == promotionDeleted.Id);


            if (albumCheckTask.Any() || movieCheckTask.Any() || gameCheckTask.Any())
            {
                return Json(new { success = false, message = "Promotion cannot be deleted because products are assigned to it" });
            }

            _unitOfWork.PromotionRepository.Remove(promotionDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Promotion deleted successfully" });
        }

        #endregion
    }
}
