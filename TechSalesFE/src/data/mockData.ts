import { Product, ProductStatus } from '../models/ui_types/product';
import { Order, OrderStatus, ProductUnit } from '../models/ui_types/order';
import { User, UserStatus } from '../models/ui_types/user';
import { Category } from '../models/ui_types/category';
import { CartItem } from '../models/ui_types/cart';
import { PaymentMethod } from '../models/ui_types/paymentMethod';
import { Address } from '../models/ui_types/address';
import { Review, ReviewStatus } from '../models/ui_types/review';
import { Voucher, VoucherType } from '../models/ui_types/voucher';
import { Notification } from '../models/ui_types/notification';
import { AuditLog } from '../models/ui_types/auditLog';
import { ReturnRequest, ReturnStatus } from '../models/ui_types/returnRequest';

export let mockCart: CartItem[] = [];

export let categories: Category[] = [
  { id: 'cat1', name: 'Laptop', productCount: 2, createdAt: '2024-01-01T00:00:00Z' },
  { id: 'cat2', name: 'Điện thoại thông minh', productCount: 2, createdAt: '2024-01-01T00:00:00Z' },
  { id: 'cat3', name: 'Màn hình máy tính', productCount: 1, createdAt: '2024-01-01T00:00:00Z' },
  { id: 'cat4', name: 'Máy chơi game & Gaming', productCount: 1, createdAt: '2024-01-01T00:00:00Z' },
  { id: 'cat5', name: 'Phụ kiện công nghệ', productCount: 3, createdAt: '2024-01-01T00:00:00Z' },
  { id: 'cat6', name: 'Thiết bị âm thanh', productCount: 1, createdAt: '2024-01-01T00:00:00Z' },
];

export let paymentMethods: PaymentMethod[] = [
  { 
    id: 'pm1', 
    name: 'Thẻ tín dụng / Ghi nợ (Credit Card)', 
    isActive: true, 
    createdAt: '2024-01-01T00:00:00Z',
    description: 'Thanh toán trực tuyến bằng thẻ Visa, Mastercard hoặc JCB'
  },
  { 
    id: 'pm2', 
    name: 'Ví điện tử (Momo / ZaloPay / VNPay)', 
    isActive: true, 
    createdAt: '2024-01-01T00:00:00Z',
    description: 'Quét mã QR thanh toán nhanh qua ví điện tử tiện lợi'
  },
  { 
    id: 'pm3', 
    name: 'Chuyển khoản ngân hàng (QR Napas)', 
    isActive: true, 
    createdAt: '2024-01-01T00:00:00Z',
    description: 'Chuyển khoản trực tiếp từ tài khoản ngân hàng nội địa'
  },
  {
    id: 'pm4',
    name: 'Thanh toán khi nhận hàng (COD)',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    description: 'Thanh toán tiền mặt trực tiếp cho nhân viên giao hàng khi nhận kiện hàng'
  }
];

export let products: Product[] = [
  {
    id: 'p1',
    name: 'MacBook Pro 16"',
    description: 'Laptop chuyên nghiệp trang bị chip Apple M3 Max, 36GB RAM, 1TB SSD, màn hình Liquid Retina XDR',
    price: 2499,
    status: ProductStatus.ACTIVE,
    stock: 45,
    imageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400',
    images: ['https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400'],
    categoryId: 'cat1',
    categoryName: 'Laptop',
    brand: 'Apple',
    rating: 4.8,
    reviewsCount: 234,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p2',
    name: 'Dell XPS 15',
    description: 'Laptop doanh nhân cao cấp Intel Core i9, 32GB RAM, 1TB SSD, màn hình 4K OLED siêu sắc nét',
    price: 1899,
    status: ProductStatus.ACTIVE,
    stock: 32,
    imageUrl: 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=400',
    images: ['https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=400'],
    categoryId: 'cat1',
    categoryName: 'Laptop',
    brand: 'Dell',
    rating: 4.6,
    reviewsCount: 187,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p3',
    name: 'iPhone 15 Pro',
    description: 'Điện thoại thông minh cao cấp khung viền Titan, chip Apple A17 Pro mạnh mẽ, bộ nhớ 256GB',
    price: 999,
    status: ProductStatus.ACTIVE,
    stock: 128,
    imageUrl: 'https://images.unsplash.com/photo-1592286927505-c0d1981d9f09?w=400',
    images: ['https://images.unsplash.com/photo-1592286927505-c0d1981d9f09?w=400'],
    categoryId: 'cat2',
    categoryName: 'Điện thoại thông minh',
    brand: 'Apple',
    rating: 4.9,
    reviewsCount: 512,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p4',
    name: 'Samsung Galaxy S24 Ultra',
    description: 'Flagship Android đỉnh cao tích hợp bút S Pen thông minh, camera 200MP, bộ nhớ trong 512GB',
    price: 1199,
    status: ProductStatus.ACTIVE,
    stock: 95,
    imageUrl: 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=400',
    images: ['https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=400'],
    categoryId: 'cat2',
    categoryName: 'Điện thoại thông minh',
    brand: 'Samsung',
    rating: 4.7,
    reviewsCount: 389,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p5',
    name: 'LG UltraWide 34"',
    description: 'Màn hình cong UltraWide 34 inch chuẩn đồ họa, độ phân giải 3440x1440, tần số quét 144Hz Nano IPS',
    price: 599,
    status: ProductStatus.ACTIVE,
    stock: 67,
    imageUrl: 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400',
    images: ['https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400'],
    categoryId: 'cat3',
    categoryName: 'Màn hình máy tính',
    brand: 'LG',
    rating: 4.5,
    reviewsCount: 156,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p6',
    name: 'Logitech MX Master 3S',
    description: 'Chuột không dây công thái học cao cấp, con lăn siêu tốc MagSpeed, cảm biến 8K DPI mọi bề mặt',
    price: 99,
    status: ProductStatus.ACTIVE,
    stock: 234,
    imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400',
    images: ['https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400'],
    categoryId: 'cat5',
    categoryName: 'Phụ kiện công nghệ',
    brand: 'Logitech',
    rating: 4.8,
    reviewsCount: 892,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p7',
    name: 'Keychron K8 Pro',
    description: 'Bàn phím cơ không dây đa kết nối, switch có thể tháo rời hot-swap, đèn nền RGB sống động',
    price: 109,
    status: ProductStatus.ACTIVE,
    stock: 156,
    imageUrl: 'https://images.unsplash.com/photo-1595225476474-87563907a212?w=400',
    images: ['https://images.unsplash.com/photo-1595225476474-87563907a212?w=400'],
    categoryId: 'cat5',
    categoryName: 'Phụ kiện công nghệ',
    brand: 'Keychron',
    rating: 4.7,
    reviewsCount: 445,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p8',
    name: 'Sony WH-1000XM5',
    description: 'Tai nghe chống ồn chủ động không dây đỉnh cao, chất âm chuẩn Hi-Res, pin 30 giờ liên tục',
    price: 399,
    status: ProductStatus.ACTIVE,
    stock: 89,
    imageUrl: 'https://images.unsplash.com/photo-1545127398-14699f92334b?w=400',
    images: ['https://images.unsplash.com/photo-1545127398-14699f92334b?w=400'],
    categoryId: 'cat6',
    categoryName: 'Thiết bị âm thanh',
    brand: 'Sony',
    rating: 4.9,
    reviewsCount: 678,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p9',
    name: 'PlayStation 5',
    description: 'Máy chơi game console thế hệ mới, hỗ trợ độ phân giải 4K 120Hz và ổ cứng SSD 825GB siêu tốc',
    price: 499,
    status: ProductStatus.ACTIVE,
    stock: 43,
    imageUrl: 'https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400',
    images: ['https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400'],
    categoryId: 'cat4',
    categoryName: 'Máy chơi game & Gaming',
    brand: 'Sony',
    rating: 4.8,
    reviewsCount: 1234,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'p10',
    name: 'AirPods Pro 2',
    description: 'Tai nghe True Wireless cao cấp, chống ồn chủ động gấp đôi, chế độ xuyên âm thích ứng thông minh',
    price: 249,
    status: ProductStatus.ACTIVE,
    stock: 178,
    imageUrl: 'https://images.unsplash.com/photo-1606841837239-c5a1a4a07af7?w=400',
    images: ['https://images.unsplash.com/photo-1606841837239-c5a1a4a07af7?w=400'],
    categoryId: 'cat5',
    categoryName: 'Phụ kiện công nghệ',
    brand: 'Apple',
    rating: 4.7,
    reviewsCount: 567,
    createdAt: '2024-01-01T00:00:00Z',
  },
];

export let mockProductUnits: ProductUnit[] = [
  // Available units for MacBook Pro 16" (p1)
  { serialNumber: 'MBP16-M3M-0901', productId: 'p1', productName: 'MacBook Pro 16"', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'MBP16-M3M-0902', productId: 'p1', productName: 'MacBook Pro 16"', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'MBP16-M3M-0903', productId: 'p1', productName: 'MacBook Pro 16"', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'MBP16-M3M-0904', productId: 'p1', productName: 'MacBook Pro 16"', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'MBP16-M3M-0905', productId: 'p1', productName: 'MacBook Pro 16"', status: 'AVAILABLE', warrantyMonths: 12 },

  // Available units for Dell XPS 15 (p2)
  { serialNumber: 'XPS15-9530-101', productId: 'p2', productName: 'Dell XPS 15', status: 'AVAILABLE', warrantyMonths: 24 },
  { serialNumber: 'XPS15-9530-102', productId: 'p2', productName: 'Dell XPS 15', status: 'AVAILABLE', warrantyMonths: 24 },
  { serialNumber: 'XPS15-9530-103', productId: 'p2', productName: 'Dell XPS 15', status: 'AVAILABLE', warrantyMonths: 24 },
  { serialNumber: 'XPS15-9530-104', productId: 'p2', productName: 'Dell XPS 15', status: 'AVAILABLE', warrantyMonths: 24 },

  // Available units for iPhone 15 Pro (p3)
  { serialNumber: 'IP15P-VN-77101', productId: 'p3', productName: 'iPhone 15 Pro', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'IP15P-VN-77102', productId: 'p3', productName: 'iPhone 15 Pro', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'IP15P-VN-77103', productId: 'p3', productName: 'iPhone 15 Pro', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'IP15P-VN-77104', productId: 'p3', productName: 'iPhone 15 Pro', status: 'AVAILABLE', warrantyMonths: 12 },

  // Available units for Samsung Galaxy S24 Ultra (p4)
  { serialNumber: 'S24U-SM-8801', productId: 'p4', productName: 'Samsung Galaxy S24 Ultra', status: 'AVAILABLE', warrantyMonths: 12 },
  { serialNumber: 'S24U-SM-8802', productId: 'p4', productName: 'Samsung Galaxy S24 Ultra', status: 'AVAILABLE', warrantyMonths: 12 },

  // Available units for LG UltraWide 34" (p5)
  { serialNumber: 'LG34-MON-3301', productId: 'p5', productName: 'LG UltraWide 34"', status: 'AVAILABLE', warrantyMonths: 24 },

  // Available units for Sony WH-1000XM5 (p8)
  { serialNumber: 'SN-SONY-XM5-1049', productId: 'p8', productName: 'Sony WH-1000XM5', status: 'SOLD', orderId: 'ORD-004', warrantyMonths: 12 },
  { serialNumber: 'SN-SONY-XM5-1050', productId: 'p8', productName: 'Sony WH-1000XM5', status: 'AVAILABLE', warrantyMonths: 12 },

  // Already assigned units for existing orders
  { serialNumber: 'IP15P-VN-66011', productId: 'p3', productName: 'iPhone 15 Pro', status: 'SOLD', orderId: 'ORD-002', warrantyMonths: 12 },
  { serialNumber: 'IP15P-VN-66022', productId: 'p3', productName: 'iPhone 15 Pro', status: 'SOLD', orderId: 'ORD-004', warrantyMonths: 12 },
  { serialNumber: 'MBP16-M3M-0811', productId: 'p1', productName: 'MacBook Pro 16"', status: 'SOLD', orderId: 'ORD-005', warrantyMonths: 12 },
];

export let orders: Order[] = [
  {
    id: 'ORD-001',
    userId: 'u4',
    status: OrderStatus.PENDING,
    totalProductAmount: 8496,
    shippingFee: 0,
    discountAmount: 0,
    totalAmount: 8496,
    customerName: 'Khách hàng Mẫu (Nguyễn Văn An)',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Thẻ tín dụng / Ghi nợ (Credit Card)',
    createdAt: new Date().toISOString(),
    items: [
      { 
        orderId: 'ORD-001', 
        productId: 'p1', 
        productName: 'MacBook Pro 16"', 
        quantity: 3, 
        price: 2499, 
        imageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400',
        serialNumbers: [] // Chưa duyệt: bắt buộc nhân viên nhập đủ 3 số serial cho 3 laptop
      },
      { 
        orderId: 'ORD-001', 
        productId: 'p3', 
        productName: 'iPhone 15 Pro', 
        quantity: 1, 
        price: 999, 
        imageUrl: 'https://images.unsplash.com/photo-1592286927505-c0d1981d9f09?w=400',
        serialNumbers: [] // Chưa duyệt: bắt buộc nhân viên nhập đủ 1 số serial cho điện thoại
      },
    ],
    payments: [
      { id: 'pay-1', paymentMethodName: 'Thẻ tín dụng / Ghi nợ (Credit Card)', status: 'PENDING', amount: 8496 }
    ]
  },
  {
    id: 'ORD-002',
    userId: 'u4',
    status: OrderStatus.APPROVED,
    totalProductAmount: 999,
    shippingFee: 0,
    discountAmount: 50,
    totalAmount: 949,
    customerName: 'Khách hàng Mẫu',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Ví điện tử (Momo / ZaloPay / VNPay)',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    items: [
      { 
        orderId: 'ORD-002', 
        productId: 'p3', 
        productName: 'iPhone 15 Pro', 
        quantity: 1, 
        price: 999, 
        imageUrl: 'https://images.unsplash.com/photo-1592286927505-c0d1981d9f09?w=400',
        serialNumbers: ['IP15P-VN-66011'] 
      },
    ],
    payments: [
      { id: 'pay-2', paymentMethodName: 'Ví điện tử (Momo / ZaloPay / VNPay)', status: 'SUCCESS', amount: 949 }
    ]
  },
  {
    id: 'ORD-003',
    userId: 'u4',
    status: OrderStatus.SHIPPING,
    totalProductAmount: 199,
    shippingFee: 20,
    discountAmount: 0,
    totalAmount: 219,
    customerName: 'Khách hàng Mẫu',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Chuyển khoản ngân hàng (QR Napas)',
    createdAt: new Date(Date.now() - 172800000).toISOString(),
    items: [
      { 
        orderId: 'ORD-003', 
        productId: 'p6', 
        productName: 'Logitech MX Master 3S', 
        quantity: 2, 
        price: 99.5, 
        imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400',
        serialNumbers: ['LOGI-MX3S-8801', 'LOGI-MX3S-8802']
      },
    ],
    payments: [
      { id: 'pay-3', paymentMethodName: 'Chuyển khoản ngân hàng (QR Napas)', status: 'SUCCESS', amount: 219 }
    ]
  },
  {
    id: 'ORD-004',
    userId: 'u4',
    status: OrderStatus.DELIVERED,
    totalProductAmount: 1398,
    shippingFee: 0,
    discountAmount: 0,
    totalAmount: 1398,
    customerName: 'Khách hàng Mẫu',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Thẻ tín dụng / Ghi nợ (Credit Card)',
    createdAt: new Date(Date.now() - 259200000).toISOString(),
    items: [
      { 
        orderId: 'ORD-004', 
        productId: 'p8', 
        productName: 'Sony WH-1000XM5', 
        quantity: 1, 
        price: 399, 
        imageUrl: 'https://images.unsplash.com/photo-1545127398-14699f92334b?w=400',
        serialNumbers: ['SN-SONY-XM5-1049']
      },
      { 
        orderId: 'ORD-004', 
        productId: 'p3', 
        productName: 'iPhone 15 Pro', 
        quantity: 1, 
        price: 999, 
        imageUrl: 'https://images.unsplash.com/photo-1592286927505-c0d1981d9f09?w=400',
        serialNumbers: ['IP15P-VN-66022']
      }
    ],
    payments: [
      { id: 'pay-4', paymentMethodName: 'Thẻ tín dụng / Ghi nợ (Credit Card)', status: 'SUCCESS', amount: 1398 }
    ]
  },
  {
    id: 'ORD-005',
    userId: 'u4',
    status: OrderStatus.DELIVERED,
    totalProductAmount: 2807,
    shippingFee: 0,
    discountAmount: 0,
    totalAmount: 2807,
    customerName: 'Khách hàng Mẫu',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Chuyển khoản ngân hàng (QR Napas)',
    createdAt: new Date(Date.now() - 345600000).toISOString(),
    items: [
      { 
        orderId: 'ORD-005', 
        productId: 'p1', 
        productName: 'MacBook Pro 16"', 
        quantity: 1, 
        price: 2499, 
        imageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400',
        serialNumbers: ['MBP16-M3M-0811']
      },
      { 
        orderId: 'ORD-005', 
        productId: 'p6', 
        productName: 'Logitech MX Master 3S', 
        quantity: 2, 
        price: 99.5, 
        imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400',
        serialNumbers: ['LOGI-MX3S-5501', 'LOGI-MX3S-5502']
      },
      { 
        orderId: 'ORD-005', 
        productId: 'p7', 
        productName: 'Keychron K8 Pro', 
        quantity: 1, 
        price: 109, 
        imageUrl: 'https://images.unsplash.com/photo-1595225476474-87563907a212?w=400',
        serialNumbers: ['KEY-K8P-7701']
      }
    ],
    payments: [
      { id: 'pay-5', paymentMethodName: 'Chuyển khoản ngân hàng (QR Napas)', status: 'SUCCESS', amount: 2807 }
    ]
  },
  {
    id: 'ORD-006',
    userId: 'u4',
    status: OrderStatus.DELIVERED,
    totalProductAmount: 3098,
    shippingFee: 0,
    discountAmount: 0,
    totalAmount: 3098,
    customerName: 'Khách hàng Mẫu',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Thẻ tín dụng / Ghi nợ (Credit Card)',
    createdAt: new Date(Date.now() - 432000000).toISOString(),
    items: [
      { 
        orderId: 'ORD-006', 
        productId: 'p2', 
        productName: 'Dell XPS 15', 
        quantity: 1, 
        price: 1899, 
        imageUrl: 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=400',
        serialNumbers: ['XPS15-9530-901']
      },
      { 
        orderId: 'ORD-006', 
        productId: 'p4', 
        productName: 'Samsung Galaxy S24 Ultra', 
        quantity: 1, 
        price: 1199, 
        imageUrl: 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=400',
        serialNumbers: ['S24U-SM-7701']
      }
    ],
    payments: [
      { id: 'pay-6', paymentMethodName: 'Thẻ tín dụng / Ghi nợ (Credit Card)', status: 'SUCCESS', amount: 3098 }
    ]
  },
  {
    id: 'ORD-007',
    userId: 'u4',
    status: OrderStatus.DELIVERED,
    totalProductAmount: 3098,
    shippingFee: 0,
    discountAmount: 0,
    totalAmount: 3098,
    customerName: 'Khách hàng Mẫu',
    shippingAddressSnapshot: '123 Đường Công Nghệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    paymentMethodName: 'Ví điện tử (Momo / ZaloPay / VNPay)',
    createdAt: new Date(Date.now() - 518400000).toISOString(),
    items: [
      { 
        orderId: 'ORD-007', 
        productId: 'p1', 
        productName: 'MacBook Pro 16"', 
        quantity: 1, 
        price: 2499, 
        imageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400',
        serialNumbers: ['MBP16-M3M-0701']
      },
      { 
        orderId: 'ORD-007', 
        productId: 'p5', 
        productName: 'LG UltraWide 34"', 
        quantity: 1, 
        price: 599, 
        imageUrl: 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400',
        serialNumbers: ['LG34-MON-2201']
      }
    ],
    payments: [
      { id: 'pay-7', paymentMethodName: 'Ví điện tử (Momo / ZaloPay / VNPay)', status: 'SUCCESS', amount: 3098 }
    ]
  },
];

export let returnRequests: ReturnRequest[] = [
  {
    id: 'RET-001',
    orderId: 'ORD-004',
    userId: 'u4',
    customerName: 'Demo Customer',
    customerPhone: '+84 901 234 567',
    productId: 'p8',
    productName: 'Sony WH-1000XM5',
    productImageUrl: 'https://images.unsplash.com/photo-1545127398-14699f92334b?w=400',
    price: 399,
    quantity: 1,
    totalRefundAmount: 399,
    serialNumber: 'SN-SONY-XM5-1049',
    orderSerialNumbers: ['SN-SONY-XM5-1049'],
    reason: 'Sản phẩm bị lỗi kỹ thuật / không hoạt động',
    description: 'Tai nghe bên phải không phát ra âm thanh sau khi sạc đầy, kết nối Bluetooth liên tục bị ngắt quãng chập chờn.',
    proofImages: [
      'https://images.unsplash.com/photo-1545127398-14699f92334b?w=400',
      'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400'
    ],
    status: ReturnStatus.PENDING_APPROVAL,
    refund: {
      amount: 399,
      method: 'Chuyển khoản Ngân hàng',
      bankName: 'Vietcombank',
      accountNumber: '0123456789',
      accountHolder: 'DEMO CUSTOMER'
    },
    createdAt: new Date(Date.now() - 14400000).toISOString(), // 4 hours ago
    updatedAt: new Date(Date.now() - 14400000).toISOString()
  },
  {
    id: 'RET-002',
    orderId: 'ORD-005',
    userId: 'u4',
    customerName: 'Khách hàng Mẫu',
    customerPhone: '+84 901 234 567',
    productId: 'p1',
    productName: 'MacBook Pro 16"',
    productImageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400',
    price: 2499,
    quantity: 1,
    totalRefundAmount: 2499,
    serialNumber: 'MBP16-M3M-0811',
    orderSerialNumbers: ['MBP16-M3M-0811'],
    reason: 'Sản phẩm khác biệt so với mô tả',
    description: 'Bản máy nhận được không đúng cấu hình RAM như đã thỏa thuận.',
    proofImages: [
      'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400'
    ],
    status: ReturnStatus.AWAITING_RETURN,
    returnAddress: 'Kho TechSales: 123 Tech Avenue, P. Bến Nghé, Q.1, TP. Hồ Chí Minh (Hotline: 1900-8888)',
    staffNote: 'Đã duyệt yêu cầu hoàn trả. Quý khách vui lòng đóng gói cẩn thận kèm hộp nguyên seal gửi về kho TechSales.',
    approvedAt: new Date(Date.now() - 86400000).toISOString(),
    returnCarrier: 'Giao Hàng Nhanh',
    returnTrackingCode: 'GHN88294012VN',
    refund: {
      amount: 949,
      method: 'E-Wallet (Momo / ZaloPay)',
      accountNumber: '0901234567',
      accountHolder: 'DEMO CUSTOMER'
    },
    createdAt: new Date(Date.now() - 100000000).toISOString(),
    updatedAt: new Date(Date.now() - 72000000).toISOString()
  },
  {
    id: 'RET-003',
    orderId: 'ORD-005',
    userId: 'u4',
    customerName: 'Khách hàng Mẫu',
    customerPhone: '+84 901 234 567',
    productId: 'p6',
    productName: 'Logitech MX Master 3S',
    productImageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400',
    price: 99.5,
    quantity: 1,
    totalRefundAmount: 99.5,
    reason: 'Sản phẩm lỗi phần cứng / nút bấm',
    description: 'Con lăn MagSpeed bị kẹt, nút bấm chuột phải không nhận tín hiệu click.',
    proofImages: [
      'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400'
    ],
    status: ReturnStatus.INSPECTING,
    returnAddress: 'Kho TechSales: 123 Tech Avenue, P. Bến Nghé, Q.1, TP. Hồ Chí Minh',
    staffNote: 'Kiện hàng đã về tới kho lúc 09:15 sáng nay. Đang tiến hành kiểm tra ngoại quan và linh kiện.',
    returnCarrier: 'Viettel Post',
    returnTrackingCode: 'VTP77391204',
    approvedAt: new Date(Date.now() - 172800000).toISOString(),
    receivedAt: new Date(Date.now() - 7200000).toISOString(),
    refund: {
      amount: 99.5,
      method: 'Chuyển khoản Ngân hàng',
      bankName: 'MB Bank',
      accountNumber: '9988776655',
      accountHolder: 'DEMO CUSTOMER'
    },
    createdAt: new Date(Date.now() - 200000000).toISOString(),
    updatedAt: new Date(Date.now() - 7200000).toISOString()
  },
  {
    id: 'RET-004',
    orderId: 'ORD-007',
    userId: 'u4',
    customerName: 'Khách hàng Mẫu',
    customerPhone: '+84 901 234 567',
    productId: 'p1',
    productName: 'MacBook Pro 16"',
    productImageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400',
    price: 2499,
    quantity: 1,
    totalRefundAmount: 2499,
    reason: 'Màn hình bị sọc kẻ khi xuất xưởng',
    description: 'Bóc máy lần đầu khởi động xuất hiện 1 vạch sọc tím chạy dọc màn hình Liquid Retina XDR.',
    proofImages: [
      'https://images.unsplash.com/photo-1517336714731-489689fd1ca4?w=400'
    ],
    status: ReturnStatus.COMPLETED,
    returnAddress: 'Kho TechSales: 123 Tech Avenue, P. Bến Nghé, Q.1, TP. Hồ Chí Minh',
    staffNote: 'Đã hoàn tất quy trình kiểm tra chất lượng và chuyển tiền hoàn lại cho quý khách.',
    returnCarrier: 'J&T Express',
    returnTrackingCode: 'JT88192039VN',
    approvedAt: new Date(Date.now() - 250000000).toISOString(),
    receivedAt: new Date(Date.now() - 86400000).toISOString(),
    inspection: {
      passed: true,
      notes: 'Kỹ thuật viên đã kiểm tra xác nhận màn hình lỗi panel gốc của Apple. Ngoại quan máy 99%, phụ kiện hộp sạc cáp đầy đủ. Đạt chuẩn hoàn tiền 100%.',
      inspectedBy: 'KTV Trần Văn Bình',
      inspectedAt: new Date(Date.now() - 50000000).toISOString()
    },
    refund: {
      amount: 2499,
      method: 'Chuyển khoản Ngân hàng',
      transactionRef: 'REF-TX-8839219',
      refundedAt: new Date(Date.now() - 43200000).toISOString(),
      bankName: 'Vietcombank',
      accountNumber: '0123456789',
      accountHolder: 'DEMO CUSTOMER'
    },
    createdAt: new Date(Date.now() - 300000000).toISOString(),
    updatedAt: new Date(Date.now() - 43200000).toISOString()
  },
  {
    id: 'RET-005',
    orderId: 'ORD-007',
    userId: 'u4',
    customerName: 'Khách hàng Mẫu',
    customerPhone: '+84 901 234 567',
    productId: 'p5',
    productName: 'LG UltraWide 34"',
    productImageUrl: 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400',
    price: 599,
    quantity: 1,
    totalRefundAmount: 599,
    reason: 'Sản phẩm khác biệt so với mô tả',
    description: 'Nhu cầu đổi sang màn hình kích thước nhỏ hơn do bàn làm việc hẹp.',
    proofImages: [
      'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400'
    ],
    status: ReturnStatus.REJECTED,
    staffNote: 'Rất tiếc chính sách TechSales không áp dụng đổi trả với lý do thay đổi nhu cầu cá nhân nếu sản phẩm không có lỗi kỹ thuật từ nhà sản xuất.',
    rejectionReason: 'Sản phẩm hoạt động bình thường, không thuộc diện đổi trả do lỗi phần cứng.',
    createdAt: new Date(Date.now() - 350000000).toISOString(),
    updatedAt: new Date(Date.now() - 340000000).toISOString()
  }
];



export let users: User[] = [
  {
    id: 'u1',
    email: 'admin@techsales.com',
    status: UserStatus.ACTIVE,
    role: 'Technical Admin',
    fullName: 'Quản trị viên Kỹ thuật',
    phone: '+84 901 000 111',
    password: 'password',
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'u2',
    email: 'business@techsales.com',
    status: UserStatus.ACTIVE,
    role: 'Business Admin',
    fullName: 'Quản lý Kinh doanh',
    phone: '+84 901 000 222',
    password: 'password',
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'u3',
    email: 'sales@techsales.com',
    status: UserStatus.ACTIVE,
    role: 'Staff',
    fullName: 'Nhân viên Bán hàng',
    phone: '+84 901 000 333',
    password: 'password',
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'u4',
    email: 'customer@email.com',
    status: UserStatus.ACTIVE,
    role: 'Customer',
    fullName: 'Khách hàng Mẫu',
    phone: '+84 901 234 567',
    password: 'password',
    createdAt: '2025-01-01T00:00:00Z',
  },
];

export let salesData = [
  { month: 'Tháng 1', revenue: 45000, orders: 234 },
  { month: 'Tháng 2', revenue: 52000, orders: 289 },
  { month: 'Tháng 3', revenue: 48000, orders: 256 },
  { month: 'Tháng 4', revenue: 61000, orders: 334 },
  { month: 'Tháng 5', revenue: 55000, orders: 298 },
  { month: 'Tháng 6', revenue: 67000, orders: 367 },
];

export let categoryData = [
  { name: 'Laptop', value: 35 },
  { name: 'Điện thoại thông minh', value: 28 },
  { name: 'Màn hình máy tính', value: 15 },
  { name: 'Phụ kiện công nghệ', value: 12 },
  { name: 'Máy chơi game & Gaming', value: 10 },
];

export let mockAddresses: Address[] = [
  {
    id: '1',
    province: 'TP. Hồ Chí Minh',
    ward: 'Phường Bến Nghé, Quận 1',
    detail: 'Số 123 Đường Công Nghệ',
    isDefault: true,
  },
  {
    id: '2',
    province: 'TP. Hồ Chí Minh',
    ward: 'Phường 12, Quận Bình Thạnh',
    detail: 'Tòa nhà Innovation Park, 456 Điện Biên Phủ',
    isDefault: false,
  }
];

export let mockReviews: Review[] = [
  {
    id: 'rev-1',
    userId: 'u4',
    userName: 'Khách hàng Mẫu',
    productId: 'p1',
    productName: 'MacBook Pro 16"',
    rating: 5,
    comment: 'Hiệu năng cực kỳ ấn tượng! Chip M3 Max xử lý đồ họa và render rất mượt mà.',
    status: ReviewStatus.VISIBLE,
    createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
    responses: [
      {
        id: 'resp-1',
        reviewId: 'rev-1',
        userId: 'u3',
        userName: 'Hỗ trợ Kỹ thuật TechSales',
        content: 'Cảm ơn quý khách đã tin tưởng và đánh giá! TechSales rất vui vì sản phẩm đáp ứng tốt nhu cầu của bạn.',
        createdAt: new Date(Date.now() - 86400000).toISOString()
      },
      {
        id: 'resp-2',
        reviewId: 'rev-1',
        userId: 'u2',
        userName: 'Bộ phận CSKH TechSales',
        content: 'TechSales luôn nỗ lực đem lại thiết bị công nghệ chính hãng chất lượng cao nhất cho khách hàng.',
        createdAt: new Date(Date.now() - 43200000).toISOString()
      }
    ]
  },
  {
    id: 'rev-2',
    userId: 'u4',
    userName: 'Khách hàng Mẫu',
    productId: 'p1',
    productName: 'MacBook Pro 16"',
    rating: 4,
    comment: 'Màn hình hiển thị xuất sắc, sắc nét và màu sắc rất chuẩn, mức giá hơi cao một chút nhưng xứng đáng.',
    status: ReviewStatus.VISIBLE,
    createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
    responses: []
  }
];

export let vouchers: Voucher[] = [
  {
    id: 'v1',
    code: 'TECHWELCOME',
    name: 'Gói Chào Mừng',
    description: 'Ưu đãi giảm trực tiếp $50 cho khách hàng lần đầu mua sắm từ $200.',
    type: VoucherType.FIXED,
    value: 50,
    maxUsage: 100,
    usedCount: 0,
    minOrderAmount: 200,
    startDate: '2026-01-01',
    endDate: '2026-12-31',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: 'v2',
    code: 'SUMMER26',
    name: 'Đón Hè Rực Rỡ',
    description: 'Giảm ngay 15% cho tất cả các thiết bị và phụ kiện công nghệ.',
    type: VoucherType.PERCENTAGE,
    value: 15,
    maxUsage: 200,
    usedCount: 45,
    minOrderAmount: 100,
    startDate: '2026-05-01',
    endDate: '2026-08-31',
    isActive: true,
    createdAt: '2026-05-01T00:00:00Z',
  },
  {
    id: 'v3',
    code: 'INDUSTRIAL',
    name: 'Chuyên Gia Công Nghệ',
    description: 'Giảm thẳng $200 cho đơn hàng thiết bị cao cấp từ $2,000.',
    type: VoucherType.FIXED,
    value: 200,
    maxUsage: 50,
    usedCount: 12,
    minOrderAmount: 2000,
    startDate: '2026-01-01',
    endDate: '2026-12-31',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
  }
];

export let mockNotifications: Notification[] = [
  {
    id: 'n1',
    userId: 'u4',
    title: 'Đơn hàng đã được khởi tạo',
    message: 'Đơn hàng #ORD-001 của bạn đã được ghi nhận và đưa vào hàng đợi xử lý vận chuyển.',
    type: 'order',
    isRead: false,
    createdAt: new Date(Date.now() - 3600000).toISOString(),
    link: '/customer/orders/ORD-001'
  },
  {
    id: 'n2',
    userId: 'u4',
    title: 'Cảnh báo bảo mật',
    message: 'Phát hiện phiên đăng nhập mới từ thiết bị tại Hà Nội, Việt Nam.',
    type: 'security',
    isRead: true,
    createdAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: 'n3',
    userId: 'u4',
    title: 'Đồng bộ hệ thống',
    message: 'Bản cập nhật phần mềm phiên bản v2.4.0 đã sẵn sàng cho thiết bị của bạn.',
    type: 'system',
    isRead: false,
    createdAt: new Date(Date.now() - 172800000).toISOString(),
  },
  {
    id: 'n4',
    userId: 'u4',
    title: 'Sản phẩm mới ra mắt',
    message: 'Các dòng màn hình đồ họa chuyên nghiệp mới đã có mặt trong danh mục. Khám phá ngay!',
    type: 'promo',
    isRead: true,
    createdAt: new Date(Date.now() - 259200000).toISOString(),
    link: '/customer/products'
  }
];

export let mockAuditLogs: AuditLog[] = [
  {
    id: 'log-1',
    createdAt: new Date().toISOString(),
    userId: 'u1',
    action: 'UPDATE',
    tableName: 'Products',
    primaryKey: 'p1',
    oldValues: '{"price": 2399}',
    newValues: '{"price": 2499}',
    affectedColumns: 'price',
  },
  {
    id: 'log-2',
    createdAt: new Date(Date.now() - 3600000).toISOString(),
    userId: 'u1',
    action: 'CREATE',
    tableName: 'Users',
    primaryKey: 'u4',
    oldValues: '',
    newValues: '{"email": "customer@email.com", "role": "Customer"}',
    affectedColumns: '*',
  },
  {
    id: 'log-3',
    createdAt: new Date(Date.now() - 7200000).toISOString(),
    userId: 'u3',
    action: 'UPDATE',
    tableName: 'Orders',
    primaryKey: 'ORD-002',
    oldValues: '{"status": "PENDING"}',
    newValues: '{"status": "APPROVED"}',
    affectedColumns: 'status',
  },
  {
    id: 'log-4',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    userId: 'u2',
    action: 'CREATE',
    tableName: 'Vouchers',
    primaryKey: 'v1',
    oldValues: '',
    newValues: '{"code": "TECHWELCOME", "value": 50}',
    affectedColumns: '*',
  },
];
