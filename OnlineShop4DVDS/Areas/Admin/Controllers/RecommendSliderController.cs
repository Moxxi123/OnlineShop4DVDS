using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Models.ViewModels;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]

    public class RecommendSliderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecommendSliderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrCreatePermission")]
        public IActionResult AddProduct()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var recommendProductList = await _unitOfWork.RecommendProductSliderRepository.GetAll(includeProperties: "Promotion,ContentType");
            return Json(new { data = recommendProductList });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var recommendProductUpdated = await _unitOfWork.RecommendProductSliderRepository.GetFirstOrDefault(r => r.Id == id);
            if (recommendProductUpdated == null)
            {
                return NotFound();
            }

            recommendProductUpdated.Status = !recommendProductUpdated.Status;
            _unitOfWork.RecommendProductSliderRepository.Update(recommendProductUpdated);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Policy = "AdminOrDeletePermission")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var recommendProductDeleted = await _unitOfWork.RecommendProductSliderRepository.GetFirstOrDefault(r => r.Id == id);
            if (recommendProductDeleted == null)
            {
                return NotFound();
            }

            _unitOfWork.RecommendProductSliderRepository.Remove(recommendProductDeleted);
            await _unitOfWork.Save();
            return Json(new { success = true, message = "Recomend product deleted successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProduct(string productType)
        {
            var albumList = await _unitOfWork.AlbumRepository.GetAll(includeProperties: "Promotion,ContentType");
            var movieList = await _unitOfWork.MovieRepository.GetAll(includeProperties: "Promotion,ContentType");
            var gameList = await _unitOfWork.GameRepository.GetAll(includeProperties: "Promotion,ContentType");

            var sliderProducts = await _unitOfWork.RecommendProductSliderRepository.GetAll();

            List<ProductListViewModel> productList = new();

            foreach (var album in albumList)
            {
                productList.Add(new ProductListViewModel
                {
                    ProductId = album.Id,
                    Name = album.Name,
                    ReleaseDate = album.ReleaseDate,
                    Price = album.Price,
                    ImageUrl = album.ImageUrl,
                    PromotionPrice = album?.PromotionPrice,
                    PromotionId = album?.PromotionId,
                    Promotion = album?.Promotion,
                    ProductType = StaticDetail.ProductType_Album,
                    ContentTypeId = album.ContentTypeId,
                    ContentType = album.ContentType,
                    IsInSlider = sliderProducts.Any(sp => sp.ProductType == StaticDetail.ProductType_Album && sp.ProductId == album.Id)
                });
            }

            foreach (var movie in movieList)
            {
                productList.Add(new ProductListViewModel
                {
                    ProductId = movie.Id,
                    Name = movie.Name,
                    ReleaseDate = movie.ReleaseDate,
                    Price = movie.Price,
                    ImageUrl = movie.ImageUrl,
                    PromotionPrice = movie?.PromotionPrice,
                    PromotionId = movie?.PromotionId,
                    Promotion = movie?.Promotion,
                    ProductType = StaticDetail.ProductType_Movie,
                    ContentTypeId = movie.ContentTypeId,
                    ContentType = movie.ContentType,
                    IsInSlider = sliderProducts.Any(sp => sp.ProductType == StaticDetail.ProductType_Movie && sp.ProductId == movie.Id)
                });
            }

            foreach (var game in gameList)
            {
                productList.Add(new ProductListViewModel
                {
                    ProductId = game.Id,
                    Name = game.Name,
                    ReleaseDate = game.ReleaseDate,
                    Price = game.Price,
                    ImageUrl = game.ImageUrl,
                    PromotionPrice = game?.PromotionPrice,
                    PromotionId = game?.PromotionId,
                    Promotion = game?.Promotion,
                    ProductType = StaticDetail.ProductType_Game,
                    ContentTypeId = game.ContentTypeId,
                    ContentType = game.ContentType,
                    IsInSlider = sliderProducts.Any(sp => sp.ProductType == StaticDetail.ProductType_Game && sp.ProductId == game.Id)
                });
            }

            var productListFilter = productList.Where(p => string.IsNullOrEmpty(productType) || p.ProductType == productType);

            return Json(new { data = productListFilter });
        }

        [HttpPost]
        public async Task<IActionResult> AddOrRemoveProductFromSlider([FromBody] AddProductToSliderRequest request)
        {
            if (request.ProductId <= 0 || string.IsNullOrEmpty(request.ProductType))
            {
                return BadRequest(new { success = false, message = "Invalid product data" });
            }

            try
            {
                // Check if the product is already in the slider
                var existingProduct = await _unitOfWork.RecommendProductSliderRepository.GetFirstOrDefault(p => p.ProductType == request.ProductType && p.ProductId == request.ProductId);
                if (existingProduct != null)
                {
                    _unitOfWork.RecommendProductSliderRepository.Remove(existingProduct);
                    await _unitOfWork.Save();
                    return Ok(new { success = true, message = "Product removed from slider successfully." });
                }

                // Check if more than 10 products are already in the slider
                var currentSliderProducts = await _unitOfWork.RecommendProductSliderRepository.GetAll();
                if (currentSliderProducts.Count() >= 10)
                {
                    return BadRequest(new { success = false, message = "Cannot add more than 10 products to the slider" });
                }

                RecommendProductSlider recommendProductSlider = null;

                switch (request.ProductType)
                {
                    case StaticDetail.ProductType_Album:
                        var album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(p => p.Id == request.ProductId, includeProperties: "Promotion,ContentType");

                        if (album == null)
                        {
                            return NotFound(new { success = false, message = "Product not found" });
                        }

                        recommendProductSlider = new RecommendProductSlider
                        {
                            ProductId = album.Id,
                            Name = album.Name,
                            Price = album.Price,
                            ImageUrl = album.ImageUrl,
                            PromotionPrice = album.PromotionPrice,
                            PromotionId = album.PromotionId,
                            ProductType = request.ProductType,
                            ContentTypeId = album.ContentTypeId,
                            Status = false
                        };

                        break;

                    case StaticDetail.ProductType_Movie:
                        var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(p => p.Id == request.ProductId, includeProperties: "Promotion,ContentType");

                        if (movie == null)
                        {
                            return NotFound(new { success = false, message = "Product not found" });
                        }

                        recommendProductSlider = new RecommendProductSlider
                        {
                            ProductId = movie.Id,
                            Name = movie.Name,
                            Price = movie.Price,
                            ImageUrl = movie.ImageUrl,
                            PromotionPrice = movie.PromotionPrice,
                            PromotionId = movie.PromotionId,
                            ProductType = request.ProductType,
                            ContentTypeId = movie.ContentTypeId,
                            Status = false
                        };

                        break;

                    case StaticDetail.ProductType_Game:
                        var game = await _unitOfWork.GameRepository.GetFirstOrDefault(p => p.Id == request.ProductId, includeProperties: "Promotion,ContentType");

                        if (game == null)
                        {
                            return NotFound(new { success = false, message = "Product not found" });
                        }

                        recommendProductSlider = new RecommendProductSlider
                        {
                            ProductId = game.Id,
                            Name = game.Name,
                            Price = game.Price,
                            ImageUrl = game.ImageUrl,
                            PromotionPrice = game.PromotionPrice,
                            PromotionId = game.PromotionId,
                            ProductType = request.ProductType,
                            ContentTypeId = game.ContentTypeId,
                            Status = false
                        };

                        break;

                    default:
                        return BadRequest(new { success = false, message = "Invalid product type" });
                }

                await _unitOfWork.RecommendProductSliderRepository.Add(recommendProductSlider);
                await _unitOfWork.Save();

                return Ok(new { success = true, message = "Product added to slider successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding or removing the product to the slider" });
            }
        }


        #endregion
    }
}
