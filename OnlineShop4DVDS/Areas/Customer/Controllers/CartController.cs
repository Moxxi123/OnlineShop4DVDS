using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModels;
using Models;
using Ultility;
using DataAccess.Repository;
using System.Security.Claims;
using Stripe.Checkout;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		private readonly IEmailSender _emailSender;

		public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
		}

		public CartItemViewModel cartItemVM { get; set; }

		public IEnumerable<CartItem> CartItems { get; set; }

		public async Task<IActionResult> Index()
		{
			List<CartItem> cartList = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();

			foreach (var item in cartList)
			{
				switch (item.ProductType)
				{
					case StaticDetail.ProductType_Album:
						item.Album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(a => a.Id == item.ProductId && a.Status == false);

						if (item.Album == null)
						{
							//add SessionCart
							HttpContext.Session.SetInt32(StaticDetail.SessionCart, cartList.Count() - 1);

							//remove product form cart
							cartList.Remove(item);

							//save cart
							HttpContext.Session.SetJson("Cart", cartList);

							return RedirectToAction("ProductNotFound");
						}

						if (item.Album != null)
						{
							item.Price = item.Album.Price;
							item.PromotionPrice = item.Album.PromotionPrice;
						}

						if (item.PromotionPrice != null || item.PromotionPrice > 0)
						{
							item.Total = (decimal)(item.Count * item.PromotionPrice);

						}
						else
						{
							item.Total = (item.Count * item.Price);
						}

						break;
					case StaticDetail.ProductType_Movie:
						item.Movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == item.ProductId && m.Status == false);

						if (item.Movie == null)
						{
							//add SessionCart
							HttpContext.Session.SetInt32(StaticDetail.SessionCart, cartList.Count() - 1);

							//remove product form cart
							cartList.Remove(item);

							//save cart
							HttpContext.Session.SetJson("Cart", cartList);

							return RedirectToAction("ProductNotFound");
						}

						if (item.Movie != null)
						{
							item.Price = item.Movie.Price;
							item.PromotionPrice = item.Movie.PromotionPrice;
						}

						if (item.PromotionPrice != null || item.PromotionPrice > 0)
						{
							item.Total = (decimal)(item.Count * item.PromotionPrice);
						}
						else
						{
							item.Total = (item.Count * item.Price);
						}
						break;
					case StaticDetail.ProductType_Game:
						item.Game = await _unitOfWork.GameRepository.GetFirstOrDefault(g => g.Id == item.ProductId && g.Status == false);

						if (item.Game == null)
						{
							//add SessionCart
							HttpContext.Session.SetInt32(StaticDetail.SessionCart, cartList.Count() - 1);

							//remove product form cart
							cartList.Remove(item);

							//save cart
							HttpContext.Session.SetJson("Cart", cartList);

							return RedirectToAction("ProductNotFound");
						}

						if (item.Game != null)
						{
							item.Price = item.Game.Price;
							item.PromotionPrice = item.Game.PromotionPrice;
						}

						if (item.PromotionPrice != null || item.PromotionPrice > 0)
						{
							item.Total = (decimal)(item.Count * item.PromotionPrice);
						}
						else
						{
							item.Total = (item.Count * item.Price);
						}
						break;
				}

			}

			CartItemViewModel cartVM = new()
			{
				CartItems = cartList,
				OrderItem = new()
			};
			//tính tổng giá cho cart
			foreach (var cart in cartVM.CartItems)
			{
				cartVM.OrderItem.OrderTotal += cart.Total;
			}

			return View(cartVM);
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

		public IActionResult ProductNotFound()
		{
			return View();
		}

		public IActionResult Minus(int productId, string productType)
		{
			List<CartItem> cartList = HttpContext.Session.GetJson<List<CartItem>>("Cart");

			CartItem? cartUpdated = cartList.FirstOrDefault(c => c.ProductType == productType && c.ProductId == productId);

			if (cartUpdated != null)
			{
				if (cartUpdated.Count <= 1)
				{
					//add SessionCart
					HttpContext.Session.SetInt32(StaticDetail.SessionCart, cartList.Count() - 1);
					//remove product form cart
					cartList.Remove(cartUpdated);
				}
				else
				{
					cartUpdated.Count -= 1;
				}
			}

			//save cart
			HttpContext.Session.SetJson("Cart", cartList);

			return RedirectToAction("Index");
		}


		public IActionResult Plus(int productId, string productType)
		{
			List<CartItem> cartList = HttpContext.Session.GetJson<List<CartItem>>("Cart");
			CartItem? cartUpdated = cartList.FirstOrDefault(c => c.ProductType == productType && c.ProductId == productId);

			if (cartUpdated != null)
			{
				cartUpdated.Count += 1;
			}

			HttpContext.Session.SetJson("Cart", cartList);

			return RedirectToAction("Index");
		}

		public IActionResult Remove(int productId, string productType)
		{
			List<CartItem> cartList = HttpContext.Session.GetJson<List<CartItem>>("Cart");
			CartItem? cartUpdated = cartList.FirstOrDefault(c => c.ProductType == productType && c.ProductId == productId);

			if (cartUpdated != null)
			{
				//add SessionCart
				HttpContext.Session.SetInt32(StaticDetail.SessionCart, cartList.Count() - 1);
				//remove product form cart
				cartList.Remove(cartUpdated);
			}

			//save cart
			HttpContext.Session.SetJson("Cart", cartList);

			return RedirectToAction("Index");
		}


		public async Task<ActionResult> Payment()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			List<CartItem> cartItems = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
			cartItemVM = new CartItemViewModel
			{
				CartItems = cartItems,
				OrderItem = new OrderItem()
			};
			cartItemVM.OrderItem.ApplicationUser = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);

			if (cartItemVM.OrderItem.ApplicationUser != null)
			{
				cartItemVM.OrderItem.Name = cartItemVM.OrderItem.ApplicationUser.Name;
				cartItemVM.OrderItem.PhoneNumber = cartItemVM.OrderItem.ApplicationUser.PhoneNumber;
				cartItemVM.OrderItem.StreetAddress = cartItemVM.OrderItem.ApplicationUser.StreetAddress;
				cartItemVM.OrderItem.City = cartItemVM.OrderItem.ApplicationUser.City;
				cartItemVM.OrderItem.State = cartItemVM.OrderItem.ApplicationUser.State;
				cartItemVM.OrderItem.PostalCode = cartItemVM.OrderItem.ApplicationUser.PostalCode;
			}
			foreach (var cart in cartItemVM.CartItems)
			{
				switch (cart.ProductType)
				{
					case StaticDetail.ProductType_Album:
						cart.Album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(m => m.Id == cart.ProductId);
						cart.Price = cart.Album?.Price ?? 0;
						cart.PromotionPrice = cart.Album?.PromotionPrice ?? 0;
						break;
					case StaticDetail.ProductType_Movie:
						cart.Movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == cart.ProductId);
						cart.Price = cart.Movie?.Price ?? 0;
						cart.PromotionPrice = cart.Movie?.PromotionPrice ?? 0;
						break;
					case StaticDetail.ProductType_Game:
						cart.Game = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == cart.ProductId);
						cart.Price = cart.Game?.Price ?? 0;
						cart.PromotionPrice = cart.Game?.PromotionPrice ?? 0;
						break;
				}

				cart.Total = cart.PromotionPrice > 0 ? cart.Count * (decimal)cart.PromotionPrice : cart.Count * cart.Price;
				cartItemVM.OrderItem.OrderTotal += cart.Total;
			}
			return View(cartItemVM);
		}

		[HttpPost]
		public async Task<ActionResult> Payment(CartItemViewModel cartVM, string paymentMethod)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			List<CartItem> cartItems = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
			cartItemVM = new CartItemViewModel
			{
				CartItems = cartItems,
				OrderItem = new OrderItem()
			};
			cartItemVM.OrderItem.OrderDate = DateTime.Now;
			cartItemVM.OrderItem.ApplicationUserId = userId;
			ApplicationUser applicationUser = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);
			foreach (var cart in cartItemVM.CartItems)
			{
				switch (cart.ProductType)
				{
					case StaticDetail.ProductType_Album:
						cart.Album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(m => m.Id == cart.ProductId);
						cart.Price = cart.Album?.Price ?? 0;
						cart.PromotionPrice = cart.Album?.PromotionPrice ?? 0;
						break;
					case StaticDetail.ProductType_Movie:
						cart.Movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == cart.ProductId);
						cart.Price = cart.Movie?.Price ?? 0;
						cart.PromotionPrice = cart.Movie?.PromotionPrice ?? 0;
						break;
					case StaticDetail.ProductType_Game:
						cart.Game = await _unitOfWork.GameRepository.GetFirstOrDefault(m => m.Id == cart.ProductId);
						cart.Price = cart.Game?.Price ?? 0;
						cart.PromotionPrice = cart.Game?.PromotionPrice ?? 0;
						break;
				}
				cart.Total = cart.PromotionPrice > 0 ? cart.Count * (decimal)cart.PromotionPrice : cart.Count * cart.Price;
				cartItemVM.OrderItem.OrderTotal += cart.Total;
			}

			cartItemVM.OrderItem.Name = applicationUser.Name;
			// Cập nhật thông tin địa chỉ và liên lạc
			cartItemVM.OrderItem.PhoneNumber = !string.IsNullOrEmpty(applicationUser.PhoneNumber)
				? applicationUser.PhoneNumber
				: cartVM.OrderItem.PhoneNumber;

			cartItemVM.OrderItem.StreetAddress = !string.IsNullOrEmpty(applicationUser.StreetAddress)
				? applicationUser.StreetAddress
				: cartVM.OrderItem.StreetAddress;

			cartItemVM.OrderItem.City = !string.IsNullOrEmpty(applicationUser.City)
				? applicationUser.City
				: cartVM.OrderItem.City;

			cartItemVM.OrderItem.State = !string.IsNullOrEmpty(applicationUser.State)
				? applicationUser.State
				: cartVM.OrderItem.State;

			cartItemVM.OrderItem.PostalCode = !string.IsNullOrEmpty(applicationUser.PostalCode)
				? applicationUser.PostalCode
				: cartVM.OrderItem.PostalCode;

			if (string.IsNullOrEmpty(cartItemVM.OrderItem.PhoneNumber) ||
				string.IsNullOrEmpty(cartItemVM.OrderItem.StreetAddress) ||
				string.IsNullOrEmpty(cartItemVM.OrderItem.City) ||
				string.IsNullOrEmpty(cartItemVM.OrderItem.State) ||
				string.IsNullOrEmpty(cartItemVM.OrderItem.PostalCode))
			{
				ModelState.AddModelError("", "Please provide all required contact and address information.");
				return View("Payment", cartItemVM);
			}

			if (paymentMethod == "PaymentCOD")
			{
				cartItemVM.OrderItem.PaymentStatus = StaticDetail.PaymentStatusDelayedPayment;
				cartItemVM.OrderItem.PaymentMethod = StaticDetail.PaymentMethodsCOD;
			}
			else if (paymentMethod == "PaymentPay")
			{
				cartItemVM.OrderItem.PaymentStatus = StaticDetail.PaymentStatusPending;
				cartItemVM.OrderItem.PaymentMethod = StaticDetail.PaymentMethodsStripe;
			}
            cartItemVM.OrderItem.OrderStatus = StaticDetail.OrderStatusPending;
            cartItemVM.OrderItem.ShippingDate = DateTime.Now.AddDays(3); // Ví dụ, đặt ngày giao hàng sau 3 ngày
            cartItemVM.OrderItem.PaymentDate = DateTime.Now;
            cartItemVM.OrderItem.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)); // Ví dụ, hạn thanh toán sau 7 ngày


            // Lưu OrderItem trước để lấy Id
            await _unitOfWork.OrderItemRepository.Add(cartItemVM.OrderItem);
			await _unitOfWork.Save();

			int totalItemCount = 0;

			// Thêm OrderDetail vào cơ sở dữ liệu và tính tổng đơn hàng
			foreach (var cart in cartItems)
			{
				var orderDetail = new OrderDetail
				{
					ProductType = cart.ProductType,
					OrderItemId = cartItemVM.OrderItem.Id, // Sử dụng Id của OrderItem vừa được lưu
					CurrentPrice = cart.PromotionPrice > 0 ? (decimal)cart.PromotionPrice : cart.Price,
					Count = cart.Count,
					TotalPrice = (cart.PromotionPrice > 0 ? (decimal)cart.PromotionPrice : cart.Price) * cart.Count

				};

				// Xác định ProductType và gán ProductId tương ứng
				switch (cart.ProductType.ToLower())
				{
					case "movie":
						orderDetail.MovieId = cart.ProductId;
						orderDetail.ProductName = cart.Movie.Name;
						break;
					case "album":
						orderDetail.AlbumId = cart.ProductId;
						orderDetail.ProductName = cart.Album.Name;
						break;
					case "game":
						orderDetail.GameId = cart.ProductId;
						orderDetail.ProductName = cart.Game.Name;
						break;
					default:
						throw new InvalidOperationException("Unknown ProductType: " + cart.ProductType);
				}

				totalItemCount += orderDetail.Count;

				await _unitOfWork.OrderDetailRepository.Add(orderDetail);
			}

			// Cập nhật CountTotal và lưu lại OrderItem
			cartItemVM.OrderItem.CountTotal = totalItemCount;

			_unitOfWork.OrderItemRepository.Update(cartItemVM.OrderItem);
			await _unitOfWork.Save();

			// Xét hình thức thanh toán
			if (paymentMethod == "PaymentCOD")
			{
				return RedirectToAction("OrderConfirmation", new { id = cartItemVM.OrderItem.Id });
			}
			else if (paymentMethod == "PaymentPay")
			{
				//
				var domain = "http://localhost:5072/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={cartItemVM.OrderItem.Id}",
					CancelUrl = domain + "Customer/Cart/Index",
					LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in cartItemVM.CartItems)
				{
					string productName;
					decimal price;
					switch (item.ProductType.ToLower())
					{
						case "movie":
							productName = item.Movie.Name;
							price = item.Movie.Price;
							break;
						case "album":
							productName = item.Album.Name;
							price = item.Album.Price;
							break;
						case "game":
							productName = item.Game.Name;
							price = item.Game.Price;
							break;
						default:
							throw new InvalidOperationException("Unknown ProductType: " + item.ProductType);
					}

					var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
					{
						PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.PromotionPrice > 0 ? (decimal)item.PromotionPrice * 100 : item.Price * 100),
							Currency = "usd",
							ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
							{
								Name = productName,
							}
						},
						Quantity = item.Count
					};

					options.LineItems.Add(sessionLineItem);
				}

				// Kiểm tra nếu danh sách LineItems không rỗng trước khi tạo phiên Stripe
				if (options.LineItems.Count == 0)
				{
					throw new InvalidOperationException("No items in cart to process payment.");
				}
				var service = new Stripe.Checkout.SessionService();

				Session session = service.Create(options);

				await _unitOfWork.OrderItemRepository.UpdateStripePaymentID(cartItemVM.OrderItem.Id, session.Id, session.PaymentIntentId);

				await _unitOfWork.Save();

				Response.Headers.Add("Location", session.Url);

				return new StatusCodeResult(303);
			}

			return RedirectToAction("OrderConfirmation", new { id = cartItemVM.OrderItem.Id });
		}

		public async Task<IActionResult> OrderConfirmation(int id)
		{
			var orderItem = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(o => o.Id == id);

			if (orderItem != null)
			{
				if (orderItem.PaymentMethod == StaticDetail.PaymentMethodsCOD)
				{
					TempData["PaymentMessage"] = "COD";
				}
				else if (orderItem.PaymentMethod == StaticDetail.PaymentMethodsStripe)
				{
					var service = new SessionService();
					Session session = service.Get(orderItem.SessionId);

					if (session.PaymentStatus.ToLower() == "paid")
					{
						await _unitOfWork.OrderItemRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
						await _unitOfWork.OrderItemRepository.UpdateStatus(id, StaticDetail.OrderStatusPending, StaticDetail.PaymentStatusApproved);
						await _unitOfWork.Save();
					}
					TempData["PaymentMessage"] = "STRIPE";
				}

				HttpContext.Session.Remove("Cart");
				HttpContext.Session.Clear();

				var user = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(a => a.Id == orderItem.ApplicationUserId);

				var orderDetailsList = await _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderItemId == orderItem.Id);

				foreach (var item in orderDetailsList)
				{
					switch (item.ProductType)
					{
						case StaticDetail.ProductType_Album:
							var album = await _unitOfWork.AlbumRepository.GetFirstOrDefault(a => a.Id == item.AlbumId, includeProperties: "Promotion,ContentType");
							if (album == null)
							{
								return RedirectToAction("ProductNotFound");
							}

							item.Album = album;
							break;

						case StaticDetail.ProductType_Movie:
							var movie = await _unitOfWork.MovieRepository.GetFirstOrDefault(m => m.Id == item.MovieId, includeProperties: "Promotion,ContentType");
							if (movie == null)
							{
								return RedirectToAction("ProductNotFound");
							}

							item.Movie = movie;
							break;

						case StaticDetail.ProductType_Game:
							var game = await _unitOfWork.GameRepository.GetFirstOrDefault(g => g.Id == item.GameId, includeProperties: "Promotion,ContentType");
							if (game == null)
							{
								return RedirectToAction("ProductNotFound");
							}

							item.Game = game;
							break;
					}
				}

				var orderConfirmationViewModel = new OrderConfirmationViewModel
				{
					OrderItem = orderItem,
					OrderDetails = orderDetailsList,
					ApplicationUser = user
				};

				// Send the email
				string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Order Confirmation</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
        }}
        .header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 20px;
            text-align: center;
            border-radius: 8px 8px 0 0;
        }}
        .content {{
            margin: 20px 0;
            line-height: 1.6;
        }}
        .footer {{
            text-align: center;
            color: #888888;
            font-size: 12px;
            margin-top: 20px;
        }}
        .section {{
            margin-bottom: 20px;
        }}
        .section h2 {{
            font-size: 18px;
            color: #333333;
            margin-bottom: 10px;
        }}
        .section p {{
            font-size: 16px;
            color: #333333;
        }}
        .highlight {{
            background-color: #f8f9fa;
            border-left: 4px solid #007bff;
            padding: 10px;
            margin-top: 10px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
        }}
        table, th, td {{
            border: 1px solid #dddddd;
        }}
        th, td {{
            padding: 8px;
            text-align: left;
        }}
        th {{
            background-color: #f2f2f2;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmation</h1>
        </div>
        <div class='content'>
            <div class='section'>
                <p>Hi {orderItem.Name},</p>
                <p>Thank you for your order. Here are the details:</p>
            </div>
            <div class='section'>
                <h2>Order Summary:</h2>
                <p>Order ID: {orderItem.Id}</p>
                <p>Order Date: {orderItem.OrderDate}</p>
                <p>Payment Method: {orderItem.PaymentMethod}</p>
                <p>Payment Status: {orderItem.PaymentStatus}</p>
            </div>
            <div class='section'>
                <h2>Order Items:</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Product Name</th>
                            <th>Quantity</th>
                            <th>Price</th>
                        </tr>
                    </thead>
                    <tbody>";

				// Iterate over orderDetailsList to populate table rows
				foreach (var item in orderDetailsList)
				{
					emailBody += $@"
                    <tr>
                        <td>{(item.ProductType == StaticDetail.ProductType_Album ? item.Album.Name : item.ProductType == StaticDetail.ProductType_Movie ? item.Movie.Name : item.Game.Name)}</td>
                        <td>{item.Count}</td>
                        <td>{item.TotalPrice}</td>
                    </tr>";
				}

				// Add the closing tags for the table and the section
				emailBody += @"
                </tbody>
            </table>
            <p><strong>Total Amount: $" + orderItem.OrderTotal + @"</strong></p>
        </div>
    </div>
    <div class='footer'>
        <p>&copy; " + DateTime.Now.Year + @" OnlineShop4DVDS. All rights reserved.</p>
    </div>
</body>
</html>";


				await _emailSender.SendEmailAsync(user.Email, "Order Confirmation", emailBody);

				return View(orderConfirmationViewModel);
			}
			else
			{
				return RedirectToAction("ProductNotFound", "Cart", new { area = "Customer" });
			}
		}

	}
}
