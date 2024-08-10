namespace Ultility
{
    public class StaticDetail
    {
        //Role
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        // Content Type
        public const string ContentType_Free = "Free Content";
        public const string ContentType_Paid = "Paid Content";

        // Promotion Status
        public const string PromotionStatus_Active = "Active";
        public const string PromotionStatus_Deactive = "Deactive";
        public const string PromotionStatus_Expired = "Expired";

        // Product Type
        public const string ProductType_Album = "Album";
        public const string ProductType_Movie = "Movie";
        public const string ProductType_Game = "Game";
        public const string ProductType_Music = "Music";

        // Price Filter
        public const string PriceFilter_LowToHigh = "Lowtohigh";
        public const string PriceFilter_HighToLow = "Hightolow";

        //Session Cart
        public const string SessionCart = "SessionCart";

        //Trạng thái thanh toán
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "Approved For Delayed Payment";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusRefund = "Refunded";

        // Phương thức thanh toán
        public const string PaymentMethodsCOD = "COD";
        public const string PaymentMethodsStripe = "STRIPE";
        // Trạng thái đơn hàng
        public const string OrderStatusPending = "Pending"; //chờ xử lý
        public const string OrderStatusProcessing = "Processing"; // đang xử lý
        public const string OrderStatusShipping = "Shipping"; // đang vận chuyển
        public const string OrderStatusDelivered = "Delivered"; // giao thành công
        public const string OrderStatusCancelled = "Cancelled"; // đã hủy
        public const string OrderStatusCancelRequest = "Cancel Request"; //yêu cầu hủy
    }
}