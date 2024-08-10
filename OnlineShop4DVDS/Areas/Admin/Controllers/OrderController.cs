using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Stripe.Climate;
using Stripe;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(o => o.Id == id, includeProperties: "ApplicationUser");

            if (orderItem == null)
            {
                return NotFound();
            }

            if(orderItem.OrderStatus == StaticDetail.OrderStatusPending)
            {
                orderItem.OrderStatus = StaticDetail.OrderStatusProcessing;
            }

            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderItem.Id == id, includeProperties: "OrderItem,Album,Movie,Game");

            if (orderDetails == null)
            {
                return NotFound();
            }

            var user = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(a => a.Id == orderItem.ApplicationUserId);

            if (user == null)
            {
                return NotFound();
            }

            OrderVM orderVM = new OrderVM()
            {
                OrderItem = orderItem,
                OrderDetails = orderDetails,
                ApplicationUser = user
            };

            return View(orderVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OrderVM orderVM)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(m => m.Id == orderVM.OrderItem.Id);

            //update các phần cần thiết của OrderItem
            orderItem.Name = orderVM.OrderItem.Name;
            orderItem.PhoneNumber = orderVM.OrderItem.PhoneNumber;
            orderItem.StreetAddress = orderVM.OrderItem.StreetAddress;
            orderItem.City = orderVM.OrderItem.City;
            orderItem.State = orderVM.OrderItem.State;
            orderItem.PostalCode = orderVM.OrderItem.PostalCode;
            orderItem.PaymentStatus = orderVM.OrderItem.PaymentStatus;
            if (!string.IsNullOrEmpty(orderVM.OrderItem.Carrier))
            {
                orderItem.Carrier = orderVM.OrderItem.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderItem.TrackingNumber))
            {
                orderItem.TrackingNumber = orderVM.OrderItem.TrackingNumber;
            }

            _unitOfWork.OrderItemRepository.Update(orderItem);
            await _unitOfWork.Save();
            TempData["success"] = "Order updated successfully";

            return RedirectToAction("Edit", new { id = orderVM.OrderItem.Id });

        }

        [HttpPost]
        public async Task<IActionResult> ShipOrder(OrderVM orderVM)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(o => o.Id == orderVM.OrderItem.Id);
            orderItem.TrackingNumber = orderVM.OrderItem.TrackingNumber;
            orderItem.Carrier = orderVM.OrderItem.Carrier;
            orderItem.OrderStatus = StaticDetail.OrderStatusShipping;
            orderItem.ShippingDate = DateTime.Now;

            //if (orderItem.PaymentStatus == StaticDetail.PaymentStatusDelayedPayment)
            //{
            //    orderItem.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            //}

            _unitOfWork.OrderItemRepository.Update(orderItem);
            await _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction("Edit", new { id = orderVM.OrderItem.Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeliveredOrder(OrderVM orderVM)
        {
            await _unitOfWork.OrderItemRepository.UpdateStatus(orderVM.OrderItem.Id, StaticDetail.OrderStatusDelivered);
            await _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction("Edit", new { id = orderVM.OrderItem.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(OrderVM orderVM)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(o => o.Id == orderVM.OrderItem.Id);

            //refund money for customer
            if (orderItem.PaymentMethod == StaticDetail.PaymentMethodsStripe)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderItem.PaymentIntendId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                //update payment status and order status
                await _unitOfWork.OrderItemRepository.UpdateStatus(orderItem.Id, StaticDetail.OrderStatusCancelled, StaticDetail.PaymentStatusRefund);
            }
            else
            {
                await _unitOfWork.OrderItemRepository.UpdateStatus(orderItem.Id, StaticDetail.OrderStatusCancelled, StaticDetail.PaymentStatusRejected);
            }

            await _unitOfWork.Save();

            TempData["Success"] = "Order Details Cancel Successfully";

            return RedirectToAction("Edit", new { id = orderVM.OrderItem.Id });
        }

        public async Task<IActionResult> Details(int? id)
        {
            var firstOrderDetail = await _unitOfWork.OrderDetailRepository.GetFirstOrDefault(m => m.Id == id, includeProperties: "OrderItem");

            if (firstOrderDetail == null)
            {
                return NotFound();
            }
            var orderItemId = firstOrderDetail.OrderItem.Id;
            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAll(m => m.OrderItem.Id == orderItemId, includeProperties: "OrderItem,Album,Movie,Game");
            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound();
            }

            return View(orderDetails);
        }

        public async Task<IActionResult> SavePaymentStatus(OrderVM orderVM)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetFirstOrDefault(o => o.Id == orderVM.OrderItem.Id);

            orderItem.PaymentStatus = orderVM.OrderItem.PaymentStatus;

            _unitOfWork.OrderItemRepository.Update(orderItem);
            await _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll(string productType)
        {
            var orderItemListBefore = await _unitOfWork.OrderItemRepository.GetAll();

            var removePaymenPendingStripe = orderItemListBefore.Where(o => o.PaymentStatus == StaticDetail.PaymentStatusPending && o.PaymentMethod == StaticDetail.PaymentMethodsStripe);

            _unitOfWork.OrderItemRepository.RemoveRange(removePaymenPendingStripe);
            await _unitOfWork.Save();

            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAll(includeProperties: "OrderItem");
            var orderList = orderDetails
                .GroupBy(o => o.OrderItem.Id)
                .Select(g => g.First())
                .Select(od => new
                {
                    orderItemId = od.OrderItem.Id,
                    orderDetailId = od.Id,
                    orderItem = new
                    {
                        name = od.OrderItem.Name,
                        orderDate = od.OrderItem.OrderDate.ToString("dd-MM-yyyy HH:mm:ss"),
                        paymentMethod = od.OrderItem.PaymentMethod,
                        paymentStatus = od.OrderItem.PaymentStatus,
                        orderStatus = od.OrderItem.OrderStatus
                    }
                })
                .ToList();

            var orderListFilter = orderList.Where(p => string.IsNullOrEmpty(productType) || p.orderItem.orderStatus == productType);

            return Json(new { data = orderListFilter });
        }

        #endregion
    }
}
