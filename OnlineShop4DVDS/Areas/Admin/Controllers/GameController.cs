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
    public class GameController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
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

            var gameProducerList = await _unitOfWork.GameProducerRepository.GetAll(m => m.Status == false);
            ViewBag.GameProducerList = new SelectList(gameProducerList, "Id", "ProducerName", "Id");

            var gameCategoryList = await _unitOfWork.GameCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.GameCategoryList = new SelectList(gameCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Game game, IFormFile? MainImage, IFormFile? File)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create game with no image
                    game.ProductType = StaticDetail.ProductType_Game;
                    await _unitOfWork.GameRepository.Add(game);
                    await _unitOfWork.Save();
                    TempData["success"] = "Game created successfully";

                    // Update ImageUrl to game
                    if (MainImage != null)
                    {
                        string ImageUrl = await UploadImage.SaveImage(game.Id, game.ProductType, MainImage, _webHostEnvironment);
                        // Update image
                        game.ImageUrl = ImageUrl;

                        _unitOfWork.GameRepository.Update(game);
                        await _unitOfWork.Save();
                    }

                    // Update FileUrl to album
                    if (File != null)
                    {
                        string FileUrl = await UploadFile.SaveFile(game.Id, game.ProductType, File, _webHostEnvironment);
                        // Update image
                        game.FileUrl = FileUrl;

                        _unitOfWork.GameRepository.Update(game);
                        await _unitOfWork.Save();
                    }
                    TempData["success"] = "Game created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("GameName", "Movie with the same name already exists.");
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

            var gameProducerList = await _unitOfWork.GameProducerRepository.GetAll(m => m.Status == false);
            ViewBag.GameProducerList = new SelectList(gameProducerList, "Id", "ProducerName", "Id");

            var gameCategoryList = await _unitOfWork.GameCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.GameCategoryList = new SelectList(gameCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");
            return View(game);
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(int? id)
        {
            var game = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == id, includeProperties: "Promotion,GameProducer,GameCategory,ContentType");

            var promotionList = await _unitOfWork.PromotionRepository.GetAll(p => p.Status == StaticDetail.PromotionStatus_Active);

            var selectList = promotionList.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            ViewBag.PromotionList = selectList;

            var gameProducerList = await _unitOfWork.GameProducerRepository.GetAll(m => m.Status == false);
            ViewBag.GameProducerList = new SelectList(gameProducerList, "Id", "ProducerName", "Id");

            var gameCategoryList = await _unitOfWork.GameCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.GameCategoryList = new SelectList(gameCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");

            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Game game, IFormFile? MainImage, IFormFile? File)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var gameUpdated = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == game.Id);
                    if (gameUpdated == null)
                    {
                        return NotFound();
                    }

                    if (MainImage != null)
                    {
                        UploadImage.DeleteImage(_webHostEnvironment, gameUpdated?.ImageUrl);

                        game.ImageUrl = await UploadImage.SaveImage(gameUpdated.Id, gameUpdated.ProductType, MainImage, _webHostEnvironment);
                    }

                    if (File != null)
                    {
                        UploadFile.DeleteFile(_webHostEnvironment, gameUpdated?.FileUrl);

                        game.FileUrl = await UploadFile.SaveFile(gameUpdated.Id, gameUpdated.ProductType, File, _webHostEnvironment);
                    }

                    _unitOfWork.GameRepository.Update(game);
                    await _unitOfWork.Save();
                    TempData["success"] = "Game update successfully";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // Unique constraint error number
                {
                    ModelState.AddModelError("GameName", "Movie with the same name already exists.");
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

            var gameProducerList = await _unitOfWork.GameProducerRepository.GetAll(m => m.Status == false);
            ViewBag.GameProducerList = new SelectList(gameProducerList, "Id", "ProducerName", "Id");

            var gameCategoryList = await _unitOfWork.GameCategoryRepository.GetAll(m => m.Status == false);
            ViewBag.GameCategoryList = new SelectList(gameCategoryList, "Id", "Name", "Id");

            var contentTypeList = await _unitOfWork.ContentTypeRepository.GetAll(c => c.Status == false);
            ViewBag.ContentTypeList = new SelectList(contentTypeList, "Id", "Type", "Id");
            return View(game);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var gameList = await _unitOfWork.GameRepository.GetAll(includeProperties: "Promotion,GameCategory,ContentType");
            return Json(new { data = gameList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var gameUpdated = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameUpdated == null)
            {
                return NotFound();
            }

            gameUpdated.Status = !gameUpdated.Status;
            _unitOfWork.GameRepository.Update(gameUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var gameDeleted = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameDeleted == null)
            {
                return NotFound();
            }

            var orderHasProduct = await _unitOfWork.OrderDetailRepository.GetAll(o => o.GameId == gameDeleted.Id && o.ProductType == StaticDetail.ProductType_Game);
            if (orderHasProduct.Any())
            {
                return Json(new { success = false, message = "Product cannot be deleted because there is an order assigned to it" });
            }

            _unitOfWork.GameRepository.Remove(gameDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Game deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFile(int id)
        {
            var gameUpdated = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == id);
            if (gameUpdated == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(gameUpdated.FileUrl))
            {
                UploadFile.DeleteFile(_webHostEnvironment, gameUpdated.FileUrl);

                gameUpdated.FileUrl = null;
            }

            _unitOfWork.GameRepository.Update(gameUpdated);
            await _unitOfWork.Save();

            return Json(new { success = true, message = "File remove successfully" });
        }

        #endregion
    }
}
