using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Models.ViewModels;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var reviewes =await _unitOfWork.ReviewRepository.GetAll(includeProperties: "Movie,Game,Album");

            return View(reviewes);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSelected([FromBody] DeleteRequest request)
        {
            // Xử lý xóa các mục đã chọn
            foreach (var id in request.Ids)
            {
                var review = await _unitOfWork.ReviewRepository.GetFirstOrDefault(c=>c.Id == id);
                if (review != null)
                {
                    _unitOfWork.ReviewRepository.Remove(review);
                }
            }
            await _unitOfWork.Save();
            return Json(new { success = true });
        }
    }
}
