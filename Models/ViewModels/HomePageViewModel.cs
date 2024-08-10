using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HomePageViewModel
    {
        public IEnumerable<MainBanner> MainBannerList { get; set; }

        public IEnumerable<ProductListViewModel> ProductListViewModel { get; set; }

        public IEnumerable<RecommendProductSlider> RecommendProductSlider { get; set; }

    }
}
