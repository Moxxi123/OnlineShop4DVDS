using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Models;

namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class NewsController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public NewsController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Index(string category)
		{
			/*var newsList = await _unitOfWork.NewsRepository.GetAll();
			if (newsList == null || !newsList.Any())
			{
				return NotFound();
			}
			return View(newsList);*/
			IEnumerable<News> newsList;

			if (string.IsNullOrEmpty(category))
			{
				newsList = await _unitOfWork.NewsRepository.GetAll();
			}
			else
			{
				newsList = await _unitOfWork.NewsRepository.GetAll(n => n.NewsCategory == category);
			}

			if (newsList == null || !newsList.Any())
			{
				return NotFound();
			}

			return View(newsList);

		}


		public async Task<IActionResult> Detail(int id)
		{
			var news = await _unitOfWork.NewsRepository.GetFirstOrDefault(m => m.Id == id);
			if (news == null)
			{
				return NotFound();
			}
			return View(new List<News> { news });
		}

		[HttpPost]
		public async Task<IActionResult> Detail(News news)
		{
			if (news == null)
			{
				return NotFound();
			}
			return RedirectToAction("Index");
		}

		#region API CALLS
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var newsList = await _unitOfWork.NewsRepository.GetAll();
			return Json(new { data = newsList });
		}
		#endregion
	}
}
