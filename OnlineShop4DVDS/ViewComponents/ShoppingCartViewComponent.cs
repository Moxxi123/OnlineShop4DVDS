using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Security.Claims;
using Ultility;

namespace OnlineShop4DVDS.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            if (HttpContext.Session.GetInt32(StaticDetail.SessionCart) != null)
            {
                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");

                //add session
                HttpContext.Session.SetInt32(StaticDetail.SessionCart, cart.Count());
            }
            return View(HttpContext.Session.GetInt32(StaticDetail.SessionCart));
        }
    }
}
