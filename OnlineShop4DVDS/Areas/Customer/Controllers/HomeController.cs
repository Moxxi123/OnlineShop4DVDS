using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModels;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var mainBannerList = await _unitOfWork.MainBannerRepository.GetAll(m => m.Status == false);

            var albumList = await _unitOfWork.AlbumRepository.GetAll(includeProperties: "Promotion,ContentType");
            var movieList = await _unitOfWork.MovieRepository.GetAll(includeProperties: "Promotion,ContentType");
            var gameList = await _unitOfWork.GameRepository.GetAll(includeProperties: "Promotion,ContentType");

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
                });
            }

            var productListFilter = productList.OrderByDescending(p => p.ReleaseDate).Take(10).ToList();

            var recommendProductSlider = await _unitOfWork.RecommendProductSliderRepository.GetAll(r => r.Status == false, includeProperties: "Promotion,ContentType");


            HomePageViewModel homePageList = new()
            {
                MainBannerList = mainBannerList,
                ProductListViewModel = productListFilter,
                RecommendProductSlider = recommendProductSlider
            };

            return View(homePageList);
        }

        public async Task<IActionResult> AddToCart(int productId, string productType)
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            CartItem cartItem = await GetCartItem(productId, productType);

            if (cartItem == null)
            {
                return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
            }

            CartItem? alreadyCart = cart.FirstOrDefault(c => c.ProductType == productType && c.ProductId == productId);

            if (alreadyCart != null)
            {
                alreadyCart.Count += cartItem.Count;
            }
            else
            {
                cart.Add(cartItem);
            }

            HttpContext.Session.SetJson("Cart", cart);
            HttpContext.Session.SetInt32(StaticDetail.SessionCart, cart.Count());

            return RedirectToAction("Index");
        }

        private async Task<CartItem> GetCartItem(int productId, string productType)
        {
            switch (productType)
            {
                case StaticDetail.ProductType_Album:
                    var album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(a => a.Id == productId, includeProperties: "Promotion,ContentType");
                    if (album == null) return null;

                    return new CartItem
                    {
                        Album = album,
                        Count = 1,
                        ProductId = album.Id,
                        ProductType = album.ProductType
                    };

                case StaticDetail.ProductType_Movie:
                    var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == productId, includeProperties: "Promotion,ContentType");
                    if (movie == null) return null;

                    return new CartItem
                    {
                        Movie = movie,
                        Count = 1,
                        ProductId = movie.Id,
                        ProductType = movie.ProductType
                    };

                case StaticDetail.ProductType_Game:
                    var game = await _unitOfWork.GameRepository.GetFirstOrDefault(g => g.Id == productId, includeProperties: "Promotion,ContentType");
                    if (game == null) return null;

                    return new CartItem
                    {
                        Game = game,
                        Count = 1,
                        ProductId = game.Id,
                        ProductType = game.ProductType
                    };

                default:
                    return null;
            }
        }

        public async Task<IActionResult> MainSearch(string search, string productType)
        {
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentProductType"] = productType;

            var albumList = await _unitOfWork.AlbumRepository.GetAll(includeProperties: "Promotion,ContentType");
            var movieList = await _unitOfWork.MovieRepository.GetAll(includeProperties: "Promotion,ContentType");
            var gameList = await _unitOfWork.GameRepository.GetAll(includeProperties: "Promotion,ContentType");

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
                });
            }

            var productListFilter = productList.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                productListFilter = productListFilter.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(productType) && productType != "All categories")
            {
                productListFilter = productListFilter.Where(p => p.ProductType.Equals(productType, StringComparison.OrdinalIgnoreCase));
            }



            return View(productListFilter.ToList());
        }
    }
}
