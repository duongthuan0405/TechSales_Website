# CartModule

Cart chỉ lưu ProductId và số lượng của người dùng lấy từ JWT.
Host đăng ký `ICartProducts` với adapter gọi `CatalogModule.Contracts.IProductCatalog`.
Thêm hoặc cập nhật giỏ kiểm tra sản phẩm tồn tại (404 nếu không có) và đang ACTIVE
(422 nếu đã ngừng kinh doanh). Xóa ảnh hưởng tới item của chính người dùng và vẫn
thực hiện được khi sản phẩm không còn bán hoặc đã bị xóa.

`GET /api/cart` trả `data.cartId` và `data.items`. Mỗi item gồm:

```json
{
  "productId": "<guid>",
  "quantity": 2,
  "createdAt": "<timestamp>",
  "updatedAt": "<timestamp>",
  "product": {
    "id": "<guid>",
    "name": "Laptop",
    "description": "Mô tả",
    "price": 15000000,
    "status": "ACTIVE",
    "brandId": "<guid>",
    "brandName": "Brand",
    "categoryId": "<guid>",
    "categoryName": "Category",
    "images": [{ "id": "<guid>", "imageUrl": "/product-images/photo.png", "isPrimary": true }]
  }
}
```

Thông tin sản phẩm được đọc theo lô, dùng giá/trạng thái mới nhất khi GET.
Sản phẩm đã xóa trả `product: null`; item vẫn còn để người dùng có thể xóa khỏi giỏ.
Sản phẩm ngừng bán vẫn có thông tin với `status: DISCONTINUED`.
Không tự giữ chỗ/trừ tồn kho khi thêm vào giỏ. Kiểm tra Catalog và transaction Cart
là hai thao tác độc lập, không đảm bảo snapshot xuyên module trong toàn bộ phiên mua hàng.

Frontend cartService hiện vẫn dùng mock; response này cung cấp dữ liệu để nối UI thật.
