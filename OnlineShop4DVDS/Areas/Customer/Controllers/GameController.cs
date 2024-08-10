using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModels;
using Models;
using Ultility;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class GameController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private object g;

        public GameController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var gameList = await _unitOfWork.GameRepository.GetAll(m => m.Status == false, includeProperties: "Promotion,ContentType");

            if (categoryId.HasValue)
            {
                gameList = gameList.Where(g => g.GameCategoryID == categoryId.Value).ToList();
            }

            ViewBag.CategoryId = categoryId;

            return View(gameList);
        }

		public async Task<IActionResult> Detail(int id)
		{
			var game = await _unitOfWork.GameRepository.GetFirstOrDefault(g => g.Id == id && g.Status == false, includeProperties: "Promotion,ContentType,GameProducer,GameCategory");
			if (game == null)
			{
				return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
			}
			CartItem cartitem = new()
			{
				Game = game,
				Count = 1,
				ProductId = game.Id,
				ProductType = game.ProductType
			};
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var applicationUser = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);
			if (applicationUser != null)
			{
				var data = from o in await _unitOfWork.OrderItemRepository.GetAll()
						   join od in await _unitOfWork.OrderDetailRepository.GetAll() on o.Id equals od.OrderItemId
						   join u in await _unitOfWork.ApplicationUserRepository.GetAll() on o.ApplicationUserId equals u.Id
						   where (od.GameId == id && o.ApplicationUserId == applicationUser.Id)
						   select new CustomerPerchasedModel
						   {
							   ApplicationUser = u,
							   OrderItem = o,
							   OrderDetail = od
						   };
				if (!data.Any())
				{
					ViewBag.CustomerPurchased = 0;
				}
				ViewBag.CustomerPurchased = data.Count();
			}
			List<Review> reviewss = (List<Review>)await _unitOfWork.ReviewRepository.GetAll(r => r.GameId == id);
			List<Review> sortedReviews = reviewss.OrderByDescending(r => r.CreateDate).ToList();
			ReviewVM reviewVM = new()
			{
				CartItem = cartitem,
				Reviews = sortedReviews
			};
			return View(reviewVM);
		}

		[HttpPost]
		public async Task<IActionResult> Detail(ReviewVM reviewVM)
		{
			List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
			CartItem? alreadyCart = cart.FirstOrDefault(c => c.ProductType == StaticDetail.ProductType_Game && c.ProductId == reviewVM.CartItem.ProductId);

			if (!reviewVM.IsReviewOnly)
			{
				if (alreadyCart != null)
				{
					alreadyCart.Count += reviewVM.CartItem.Count;
				}
				else
				{
					cart.Add(reviewVM.CartItem);
				}
				HttpContext.Session.SetJson("Cart", cart);
				HttpContext.Session.SetInt32(StaticDetail.SessionCart, cart.Count());
				return RedirectToAction("Index");
			}
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var applicationUser = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);

			var ReviewAll = await _unitOfWork.ReviewRepository.GetAll(includeProperties: "Game");

			var alreadyReviewExists = ReviewAll.Any(r => r.ApplicationUserId == userId && r.GameId == reviewVM.CartItem.ProductId);
			if (!alreadyReviewExists)
			{
				var newReview = new Review
				{
					ApplicationUserId = userId,
					CreateDate = DateTime.Now,
					Name = applicationUser.Name,
					StarRate = reviewVM.CurrentUserReview.StarRate,
					Content = reviewVM.CurrentUserReview.Content,
					GameId = reviewVM.CartItem.ProductId,
					ProductName = reviewVM.CartItem.Game.Name,
					ProductType = reviewVM.CartItem.ProductType
				};
				await _unitOfWork.ReviewRepository.Add(newReview);
				await _unitOfWork.Save();
			}
			else
			{
				TempData["swalerror"] = "You have already reviewed this product.";
			}
			return RedirectToAction("Detail", new { id = reviewVM.CartItem.ProductId });
		}



		public async Task<IActionResult> AddToCart(int Id)
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            CartItem? alreadyCart = cart.FirstOrDefault(c => c.ProductType == StaticDetail.ProductType_Game && c.ProductId == Id);

            var game = await _unitOfWork.GameRepository.GetFirstOrDefault(g => g.Id == Id, includeProperties: "Promotion,ContentType");
            if (game == null)
            {
                return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
            }

            CartItem cartitem = new()
            {
                Game = game,
                Count = 1,
                ProductId = game.Id,
                ProductType = game.ProductType
            };


            if (alreadyCart == null)
            {
                cart.Add(cartitem);
            }
            else
            {
                alreadyCart.Count += 1;
            }

            // Save cart
            HttpContext.Session.SetJson("Cart", cart);

            // Add SessionCart
            HttpContext.Session.SetInt32(StaticDetail.SessionCart, cart.Count());

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var game = await _unitOfWork.GameRepository.GetFirstOrDefault(g => g.Id == id, includeProperties: "Promotion,GameCategory,ContentType,GameProducer");
            if (game == null)
            {
                return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
            }

            CartItem cartitem = new()
            {
                Game = game,
                Count = 1,
                ProductId = game.Id,
                ProductType = game.ProductType
            };

            if (game.FileUrl == null)
            {
                TempData["swalerror"] = "Your download is not available";
                return RedirectToAction("Detail", new { id = id });
            }

            string? fullPath = UploadFile.GetFilePath(_webHostEnvironment, game.FileUrl);

            if (fullPath == null || !System.IO.File.Exists(fullPath))
            {
                TempData["swalerror"] = "Your download is not available";
                return RedirectToAction("Detail", new { id = id });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.CopyTo(memory);
            }

            memory.Position = 0;
            string fileName = Path.GetFileName(fullPath);
            string contentType = "application/rar"; // Change file type

            return File(memory, contentType, fileName);
        }

        #region API CALLS
        [HttpPost]
        public async Task<IActionResult> GetFilterProduct([FromBody] FilterRequestModel filter)
        {
            var gameList = await _unitOfWork.GameRepository.GetAll(includeProperties: "Promotion,GameCategory,ContentType,GameProducer");

            if (filter.ContentTypeId.Length > 0)
            {
                gameList = gameList.Where(m => filter.ContentTypeId.Contains(m.ContentTypeId)).ToList();
            }

            if (filter.CategoryId.Length > 0)
            {
                gameList = gameList.Where(m => filter.CategoryId.Contains(m.GameCategoryID)).ToList();
            }

            if (filter.ProducerId.Length > 0)
            {
                gameList = gameList.Where(m => filter.ProducerId.Contains(m.GameProducerId)).ToList();
            }

            gameList = gameList.Where(m => m.Price >= filter.MinPrice && m.Price <= filter.MaxPrice).ToList();

            if (filter.SortOrder == StaticDetail.PriceFilter_LowToHigh)
            {
                gameList = gameList.OrderBy(m => m.Price).ToList();
            }
            else if (filter.SortOrder == StaticDetail.PriceFilter_HighToLow)
            {
                gameList = gameList.OrderByDescending(m => m.Price).ToList();
            }

            var result = gameList.Select(m => new {
                m.Id,
                m.Name,
                m.ImageUrl,
                m.Price,
                PromotionPrice = m.Promotion != null ? m.PromotionPrice : (decimal?)null,
                Promotion = m.Promotion != null ? new { m.Promotion.DiscountPercent } : null,
                ContentType = new { m.ContentType.Type }
            }).ToList();

            return Json(new { data = result });
        }

        #endregion
    }
}
