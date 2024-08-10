using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Models;
using Models.ViewModels;
using System.Security.Claims;
using Ultility;

namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class MovieController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MovieController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var movieList = await _unitOfWork.MovieRepository.GetAll(m => m.Status == false, includeProperties: "Promotion,ContentType");

            if (categoryId.HasValue)
            {
                movieList = movieList.Where(m => m.MovieCategoryID == categoryId.Value).ToList();
            }

            ViewBag.CategoryId = categoryId;

            return View(movieList);
        }

		public async Task<IActionResult> Detail(int id)
		{
			var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == id, includeProperties: "Promotion,ContentType,MovieProducer,MovieCategory");
			if (movie == null)
			{
				return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
			}
			CartItem cartitem = new()
			{
				Movie = movie,
				Count = 1,
				ProductId = movie.Id,
				ProductType = movie.ProductType
			};

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var applicationUser = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);
			if (applicationUser != null)
			{
				var data = from o in await _unitOfWork.OrderItemRepository.GetAll()
						   join od in await _unitOfWork.OrderDetailRepository.GetAll() on o.Id equals od.OrderItemId
						   join u in await _unitOfWork.ApplicationUserRepository.GetAll() on o.ApplicationUserId equals u.Id
						   where (od.MovieId == id && o.ApplicationUserId == applicationUser.Id)
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

			List<Review> reviewss = (List<Review>)await _unitOfWork.ReviewRepository.GetAll(r => r.MovieId == id);
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
			if (!reviewVM.IsReviewOnly)
			{
				List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
				CartItem? alreadyCart = cart.FirstOrDefault(c => c.ProductType == StaticDetail.ProductType_Movie && c.ProductId == reviewVM.CartItem.ProductId);
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

			var ReviewAll = await _unitOfWork.ReviewRepository.GetAll(includeProperties: "Movie");

			var alreadyReviewExists = ReviewAll.Any(r => r.ApplicationUserId == userId && r.MovieId == reviewVM.CartItem.ProductId);
			if (!alreadyReviewExists)
			{
				var newReview = new Review
				{
					ApplicationUserId = userId,
					CreateDate = DateTime.Now,
					Name = applicationUser.Name,
					StarRate = reviewVM.CurrentUserReview.StarRate,
					Content = reviewVM.CurrentUserReview.Content,
					MovieId = reviewVM.CartItem.ProductId,
					ProductName = reviewVM.CartItem.Movie.Name,
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
            CartItem? alreadyCart = cart.FirstOrDefault(c => c.ProductType == StaticDetail.ProductType_Movie && c.ProductId == Id);

            var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == Id, includeProperties: "Promotion,ContentType");
            if (movie == null)
            {
                return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
            }

            CartItem cartitem = new()
            {
                Movie = movie,
                Count = 1,
                ProductId = movie.Id,
                ProductType = movie.ProductType
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
            var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == id, includeProperties: "Promotion,MovieCategory,ContentType,MovieProducer");
            if (movie == null)
            {
                return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
            }

            CartItem cartitem = new()
            {
                Movie = movie,
                Count = 1,
                ProductId = movie.Id,
                ProductType = movie.ProductType
            };

            if (movie.FileUrl == null)
            {
                TempData["swalerror"] = "Your download is not available";
                return RedirectToAction("Detail", new { id = id });
            }

            string? fullPath = UploadFile.GetFilePath(_webHostEnvironment, movie.FileUrl);

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
            var movieList = await _unitOfWork.MovieRepository.GetAll(includeProperties: "Promotion,MovieCategory,ContentType,MovieProducer");

            if (filter.ContentTypeId.Length > 0)
            {
                movieList = movieList.Where(m => filter.ContentTypeId.Contains(m.ContentTypeId)).ToList();
            }

            if (filter.CategoryId.Length > 0)
            {
                movieList = movieList.Where(m => filter.CategoryId.Contains(m.MovieCategoryID)).ToList();
            }

            if (filter.ProducerId.Length > 0)
            {
                movieList = movieList.Where(m => filter.ProducerId.Contains(m.MovieProducerId)).ToList();
            }

            movieList = movieList.Where(m => m.Price >= filter.MinPrice && m.Price <= filter.MaxPrice).ToList();

            if (filter.SortOrder == StaticDetail.PriceFilter_LowToHigh)
            {
                movieList = movieList.OrderBy(m => m.Price).ToList();
            }
            else if (filter.SortOrder == StaticDetail.PriceFilter_HighToLow)
            {
                movieList = movieList.OrderByDescending(m => m.Price).ToList();
            }

            var result = movieList.Select(m => new {
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
