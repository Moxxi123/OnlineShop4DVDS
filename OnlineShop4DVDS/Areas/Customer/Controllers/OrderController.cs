using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Newtonsoft.Json;
using System.Security.Claims;
using Ultility;
namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser applicationUser = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);
            var orderItemListBefore = await _unitOfWork.OrderItemRepository.GetAll(o => o.ApplicationUserId == applicationUser.Id);

            var removePaymenPendingStripe = orderItemListBefore.Where(o => o.PaymentStatus == StaticDetail.PaymentStatusPending && o.PaymentMethod == StaticDetail.PaymentMethodsStripe);

            _unitOfWork.OrderItemRepository.RemoveRange(removePaymenPendingStripe);
            await _unitOfWork.Save();

            var orderItemListAfter = await _unitOfWork.OrderItemRepository.GetAll(o => o.ApplicationUserId == applicationUser.Id);

            return View(orderItemListAfter);
        }
        public async Task<IActionResult> Details(int? id)
        {
            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAll(m => m.OrderItem.Id == id, includeProperties: "OrderItem,Album,Movie,Game");
            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound();
            }
            return View(orderDetails);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(int id, string cancelRequest)
        {
            var orderItemUpdated = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(o => o.Id == id);

            if (orderItemUpdated == null)
            {
                return NotFound();
            }

            orderItemUpdated.CancelRequest = cancelRequest;
            orderItemUpdated.OrderStatus = StaticDetail.OrderStatusCancelRequest;

            _unitOfWork.OrderItemRepository.Update(orderItemUpdated);
            await _unitOfWork.Save();
            TempData["swalsuccess"] = "Your cancel request has been sent";
            return RedirectToAction("Index");
        }
    }
}
