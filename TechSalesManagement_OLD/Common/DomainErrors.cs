namespace TechSalesManagement.Common;

public static class DomainErrors
{
    public static class Product
    {
        public const string NameRequired = "Tên sản phẩm không được để trống.";
        public const string BrandRequired = "Thương hiệu không được để trống.";
        public const string PriceNegative = "Giá sản phẩm không được âm.";
    }

    public static class Category
    {
        public const string NameRequired = "Tên danh mục không được để trống.";
    }

    public static class User
    {
        public const string EmailInvalid = "Email không hợp lệ.";
        public const string PasswordRequired = "Mật khẩu không được để trống.";
    }

    public static class UserProfile
    {
        public const string FullNameRequired = "Họ tên không được để trống.";
        public const string PhoneRequired = "Số điện thoại không được để trống.";
    }

    public static class Inventory
    {
        public const string InsufficientStock = "Không đủ hàng trong kho để giữ.";
    }

    public static class Role
    {
        public const string NameRequired = "Tên Role không được để trống.";
    }

    public static class Permission
    {
        public const string CodeRequired = "Mã quyền không được để trống.";
    }

    public static class ReviewResponse
    {
        public const string ContentRequired = "Nội dung phản hồi không được để trống.";
    }
}
