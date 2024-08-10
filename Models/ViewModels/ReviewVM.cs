using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ReviewVM
    {
        public CartItem? CartItem { get; set; }
        public List<Review>? Reviews { get; set; } // Danh sách tất cả các đánh giá
        public Review? CurrentUserReview { get; set; } // Đánh giá của người dùng hiện tại
        public bool IsReviewOnly { get; set; }
    }
}
