using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class ChartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public ChartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public IActionResult IndexCO()
        {
            return View();
        }
        public async Task<IActionResult> ChartOrderQuantityDay(DateTime? day1, DateTime? day2)
        {
            var orders = await _unitOfWork.OrderItemRepository.GetAll();
            if (!day1.HasValue)
            {
                return Json(new { error = "Ngày bắt đầu (day1) không được để trống." });
            }
            var startDate = day1.Value.Date;
            var endDate = day2.HasValue ? day2.Value.Date : startDate;

            var filteredOrders = orders.Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate).ToList();

            var ordersByDay = filteredOrders
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                }).ToList();

            var labels = ordersByDay.Select(x => x.Date.ToString("dd/MM/yyyy")).ToList();

            var quantities = ordersByDay.Select(x => x.Count).ToList();

            var dataChart = new
            {
                labels = labels,
                quantities = quantities
            };

            return Json(dataChart);
        }
        public async Task<IActionResult> ChartOrderQuantityMonth(int? month, int? year)
        {
            var orders = await _unitOfWork.OrderItemRepository.GetAll();

            // Nếu tháng không được cung cấp, trả về lỗi
            if (!month.HasValue)
            {
                return Json(new { error = "Tháng không được để trống." });
            }

            // Nếu năm không được cung cấp, sử dụng năm hiện tại
            var currentYear = DateTime.Now.Year;
            var selectedYear = year.HasValue ? year.Value : currentYear;

            var startDate = new DateTime(selectedYear, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var filteredOrders = orders
                .Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate)
                .ToList();

            // Tính tổng số lượng đơn hàng trong tháng
            var totalOrdersInMonth = filteredOrders.Count;

            // Tạo danh sách nhãn và số lượng cho đồ thị
            var labels = new List<string> { startDate.ToString("MM/yyyy") };
            var quantities = new List<int> { totalOrdersInMonth };

            var dataChart = new
            {
                labels = labels,
                quantities = quantities
            };

            return Json(dataChart);
        }

        public async Task<IActionResult> ChartOrderQuantityYear(int? year)
        {
            var orders = await _unitOfWork.OrderItemRepository.GetAll();

            // Nếu năm không được cung cấp, trả về lỗi
            if (!year.HasValue)
            {
                return Json(new { error = "Năm không được để trống." });
            }

            // Xác định năm
            var selectedYear = year.Value;
            var startDate = new DateTime(selectedYear, 1, 1);
            var endDate = startDate.AddYears(1).AddDays(-1);

            // Lọc các đơn hàng theo năm
            var filteredOrders = orders
                .Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate)
                .ToList();

            // Tính tổng số lượng đơn hàng trong năm
            var totalOrdersInYear = filteredOrders.Count;

            // Tạo danh sách nhãn và số lượng cho đồ thị
            var labels = new List<string> { selectedYear.ToString() };
            var quantities = new List<int> { totalOrdersInYear };

            var dataChart = new
            {
                labels = labels,
                quantities = quantities
            };

            return Json(dataChart);
        }

        public IActionResult IndexCDT()
        {
            return View();
        }
        public async Task<IActionResult> ChartOrderCDTDay(DateTime? day1, DateTime? day2)
        {
			var ordersFilter = await _unitOfWork.OrderItemRepository.GetAll();

			var orders = ordersFilter.Where(x => x.OrderStatus == StaticDetail.OrderStatusDelivered);

			if (!day1.HasValue)
            {
                return Json(new { error = "Ngày bắt đầu (day1) không được để trống." });
            }

            var startDate = day1.Value.Date;
            var endDate = day2.HasValue ? day2.Value.Date : startDate;

            // Lọc đơn hàng trong khoảng ngày
            var filteredOrders = orders.Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate).ToList();

            // Nhóm theo ngày và tính tổng OrderTotal cho mỗi ngày
            var ordersByDay = filteredOrders
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalOrderAmount = g.Sum(x => x.OrderTotal) // Tính tổng OrderTotal cho mỗi ngày
                }).ToList();

            // Tạo danh sách nhãn và số liệu cho biểu đồ
            var labels = ordersByDay.Select(x => x.Date.ToString("dd/MM/yyyy")).ToList();
            var totals = ordersByDay.Select(x => x.TotalOrderAmount).ToList();

            var dataChart = new
            {
                labels = labels,
                totals = totals
            };

            return Json(dataChart);
        }

        public async Task<IActionResult> ChartOrderCDTMonth(int? month, int? year)
        {
			var ordersFilter = await _unitOfWork.OrderItemRepository.GetAll();

			var orders = ordersFilter.Where(x => x.OrderStatus == StaticDetail.OrderStatusDelivered);

			// Nếu tháng không được cung cấp, trả về lỗi
			if (!month.HasValue)
            {
                return Json(new { error = "Tháng không được để trống." });
            }

            // Nếu năm không được cung cấp, sử dụng năm hiện tại
            var currentYear = DateTime.Now.Year;
            var selectedYear = year.HasValue ? year.Value : currentYear;

            // Xác định khoảng thời gian của tháng được chọn
            var startDate = new DateTime(selectedYear, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Lọc đơn hàng trong khoảng thời gian tháng
            var filteredOrders = orders
                .Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate)
                .ToList();

            // Tính tổng doanh thu cho toàn bộ tháng
            var totalOrderAmountForMonth = filteredOrders
                .Sum(x => x.OrderTotal);

            var labels = new List<string> { startDate.ToString("MM/yyyy") };
            var totals = new List<decimal> { totalOrderAmountForMonth };

            var dataChart = new
            {
                labels = labels,
                totals = totals
            };

            return Json(dataChart);
        }

        public async Task<IActionResult> ChartOrderCDTYear(int? year)
        {
			var ordersFilter = await _unitOfWork.OrderItemRepository.GetAll();

			var orders = ordersFilter.Where(x => x.OrderStatus == StaticDetail.OrderStatusDelivered);

			// Nếu năm không được cung cấp, trả về lỗi
			if (!year.HasValue)
            {
                return Json(new { error = "Năm không được để trống." });
            }

            // Xác định năm
            var selectedYear = year.Value;
            var startDate = new DateTime(selectedYear, 1, 1);
            var endDate = startDate.AddYears(1).AddDays(-1);

            // Lọc các đơn hàng theo năm
            var filteredOrders = orders
                .Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate)
                .ToList();

            // Tính tổng doanh thu cho năm
            var totalOrderAmountForYear = filteredOrders
                .Sum(x => x.OrderTotal);

            // Tạo danh sách nhãn và số lượng cho đồ thị
            var labels = new List<string> { selectedYear.ToString() };
            var totals = new List<decimal> { totalOrderAmountForYear };

            var dataChart = new
            {
                labels = labels,
                totals = totals
            };

            return Json(dataChart);
        }

    }
}
