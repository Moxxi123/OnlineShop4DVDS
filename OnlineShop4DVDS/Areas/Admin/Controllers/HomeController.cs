using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.OrderItemRepository.GetAll();

            // Lấy năm và tháng hiện tại
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            // Lọc đơn hàng theo tháng hiện tại
            var monthlyOrders = orders.Where(o => o.OrderDate.Year == currentYear && o.OrderDate.Month == currentMonth);

            // Tính tổng OrderTotal cho các đơn hàng đã lọc
            var totalOrderTotalMonth = monthlyOrders.Sum(o => o.OrderTotal);

            // Gửi tổng số tiền theo tháng đến view
            ViewBag.totalOrderTotalMonth = totalOrderTotalMonth;

            // Lọc đơn hàng theo năm hiện tại
            var yearlyOrders = orders.Where(o => o.OrderDate.Year == currentYear);

            // Tính tổng OrderTotal cho các đơn hàng đã lọc
            var totalOrderTotalyear = yearlyOrders.Sum(o => o.OrderTotal);

            // Gửi tổng số tiền theo năm đến view
            ViewBag.totalOrderTotalyear = totalOrderTotalyear;

            // Tính tổng số lượng đơn hàng
            var totalOrders = orders.Count();

            // Sử dụng ViewBag để truyền dữ liệu đến view
            ViewBag.TotalOrders = totalOrders;


            var album = await _unitOfWork.AlbumRepository.GetAll();

            var movie = await _unitOfWork.MovieRepository.GetAll();

            var game = await _unitOfWork.GameRepository.GetAll();

            // Tính số lượng sản phẩm từ mỗi loại
            var albumCount = album.Count();
            var movieCount = movie.Count();
            var gameCount = game.Count();

            // Tính tổng số lượng sản phẩm
            var totalProductCount = albumCount + movieCount + gameCount;

            ViewBag.TotalProductCount = totalProductCount;


            return View();
        }


        public async Task<IActionResult> ChartDoughnut()
        {
            var charts = await _unitOfWork.OrderItemRepository.GetAll();
            if (charts == null)
            {
                return Json(new { labels = new List<string>(), quantities = new List<int>() });
            }
            var groupedOrderStatus = charts
                .GroupBy(p => p.OrderStatus)
                .Select(g => new
                {
                    OrderStatus = g.Key,
                    Count = g.Count()
                }).ToList();

            var labels = groupedOrderStatus.Select(x => x.OrderStatus).ToList();
            var quantities = groupedOrderStatus.Select(x => x.Count).ToList();

            ViewBag.GroupedOrderStatus = groupedOrderStatus;

            var dataChart = new
            {
                labels = labels,
                quantities = quantities
            };

            return Json(dataChart);
        }
        public async Task<IActionResult> ChartOrder()
        {
            var charts = await _unitOfWork.OrderDetailRepository.GetAll(includeProperties: "OrderItem,Album,Game,Movie");

/*            var charFilter =  charts.Where(c => c.OrderItem.OrderStatus == StaticDetail.OrderStatusDelivered);*/


            var labels = charts.Select(x => x.ProductName).ToList();

            var quantities = charts.Select(x => x.Count).ToList();

            var dataChart = new
            {
                labels = labels,
                quantities = quantities
            };

            return Json(dataChart);
        }

    }
}
