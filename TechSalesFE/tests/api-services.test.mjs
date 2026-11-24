import test from 'node:test';
import assert from 'node:assert/strict';
import { build } from 'esbuild';

const modules = {};
for (const name of ['cart', 'category', 'address', 'voucher', 'user', 'payment', 'product', 'order', 'inventory', 'shipping', 'auth', 'return', 'repair', 'review', 'notification', 'dashboard']) {
  const result = await build({ entryPoints: [`src/services/${name}Service.ts`], bundle: true,
    write: false, format: 'esm', platform: 'browser', define: { 'import.meta.env': '{}' } });
  modules[name] = (await import(`data:text/javascript;base64,${Buffer.from(result.outputFiles[0].text).toString('base64')}`))[`${name}Service`];
}
let calls = [];
function backend(handler) {
  calls = [];
  const values = new Map([['token', 'real-token'], ['tech_sales_user', JSON.stringify({ id: 'me', role: 'Technical Admin' })]]);
  globalThis.localStorage = { getItem: key => values.get(key) ?? null,
    setItem: (key, value) => values.set(key, value), removeItem: key => values.delete(key) };
  globalThis.window = { dispatchEvent() {} };
  globalThis.fetch = async (url, options) => {
    const call = { url, ...options };
    calls.push(call);
    const result = await handler(call);
    return new Response(JSON.stringify(result?.status ? { success: false, message: 'Backend error', errors: { quantity: ['invalid'] } }
      : { success: true, data: result }), { status: result?.status ?? 200, headers: { 'Content-Type': 'application/json' } });
  };
}
const productId = '11111111-1111-4111-8111-111111111111';
test('account creation sends every selected role', async () => {
  backend(call => ({ id: 'u', ...JSON.parse(call.body), profile: { fullName: '', phone: '' } }));
  await modules.user.createUser({ email: 'multi@example.com', password: 'Example-password1!', role: 'Staff', roles: ['Staff', 'Customer'] });
  assert.deepEqual(JSON.parse(calls[0].body).roles, ['Staff', 'Customer']);
});

test('account editing assigns the full selected role set including additions and removals', async () => {
  let assigned = ['Staff', 'Customer'];
  backend(call => {
    if (call.url === '/api/admin/rbac/roles') return [
      { id: 'staff-id', name: 'Staff' }, { id: 'customer-id', name: 'Customer' }, { id: 'business-id', name: 'Business Admin' },
    ];
    if (call.url === '/api/admin/rbac/users/u/roles') {
      const names = { 'staff-id': 'Staff', 'customer-id': 'Customer', 'business-id': 'Business Admin' };
      assigned = JSON.parse(call.body).roleIds.map(id => names[id]);
      return null;
    }
    return { items: [{ id: 'u', email: 'multi@example.com', roles: [...assigned], status: 'ACTIVE', profile: { fullName: '', phone: '' } }], totalCount: 1, pageSize: 100 };
  });
  await modules.user.updateUser('u', { roles: ['Customer', 'Business Admin'], role: 'Customer' });
  const assignment = calls.find(call => call.url === '/api/admin/rbac/users/u/roles');
  assert.deepEqual(JSON.parse(assignment.body), { roleIds: ['customer-id', 'business-id'] });
  calls = [];
  await modules.user.updateUser('u', { roles: ['Business Admin', 'Customer'] });
  assert.ok(calls.every(call => call.method === 'GET'));
  calls = [];
  await assert.rejects(modules.user.updateUser('u', { roles: [] }), error => error.status === 400);
  assert.ok(calls.every(call => call.method === 'GET'));
});
test('technical pagination uses one unified server page for all roles and sends search with role filters', async () => {
  backend(() => ({ items: [], pageNumber: 3, pageSize: 10, totalCount: 45 }));
  const result = await modules.user.getAccountPage('all', { pageNumber: 3, pageSize: 10, role: 'all', keyword: 'name' });
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/admin/users?pageNumber=3&pageSize=10&keyword=name');
  assert.equal(result.totalCount, 45);
  await modules.user.getAccountPage('all', { pageNumber: 1, pageSize: 20, role: 'Customer', keyword: 'email' });
  assert.equal(calls[1].url, '/api/admin/users?pageNumber=1&pageSize=20&keyword=email&role=Customer');
});
test('technical user search sends keyword to both server directories and preserves server matches', async () => {
  backend(call => ({ items: [{ id: call.url.includes('/customers?') ? 'customer' : 'staff',
    email: 'server-result@example.com', roles: ['Customer'], status: 'ACTIVE',
    profile: { fullName: 'Server result', phone: '' } }], totalCount: 1, pageSize: 100 }));
  const users = await modules.user.getUsers('all', ' Nguyen & Co ');
  assert.equal(users.length, 2);
  assert.equal(calls.length, 2);
  for (const call of calls) assert.equal(new URL(call.url, 'https://test.local').searchParams.get('keyword'), 'Nguyen & Co');
  calls = [];
  await modules.user.getUsers('Staff', 'email@example.com');
  assert.equal(calls.length, 1);
  const query = new URL(calls[0].url, 'https://test.local').searchParams;
  assert.equal(query.get('role'), 'Staff');
  assert.equal(query.get('keyword'), 'email@example.com');
});
test('account directories fetch one backend page and forward search and staff role', async () => {
  backend(() => ({ items: [{ id: 'paged-user', email: 'match@example.com', roles: ['Customer'],
    status: 'ACTIVE', profile: { fullName: 'Matched user', phone: '123' } }],
    pageNumber: 2, pageSize: 10, totalCount: 25 }));
  const result = await modules.user.getAccountPage('customers', { pageNumber: 2, pageSize: 10, keyword: ' match ' });
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/admin/users/customers?pageNumber=2&pageSize=10&keyword=match');
  assert.equal(result.totalCount, 25);
  assert.equal(result.items[0].fullName, 'Matched user');
  await modules.user.getAccountPage('staff', { pageNumber: 1, pageSize: 20, role: 'Staff' });
  assert.equal(calls[1].url, '/api/admin/users/staff?pageNumber=1&pageSize=20&role=Staff');
});
test('deactivating an account uses the dedicated endpoint and preserves authorization errors', async () => {
  backend(() => null);
  await modules.user.deactivateUser('target');
  assert.equal(calls[0].url, '/api/admin/users/target/deactivate');
  assert.equal(calls[0].method, 'POST');
  assert.equal(calls[0].headers.get('Authorization'), 'Bearer real-token');
  backend(() => ({ status: 403 }));
  await assert.rejects(modules.user.deactivateUser('admin'), error => error.status === 403);
});
test('change password calls the authenticated API and propagates rejected passwords', async () => {
  backend(() => null);
  await modules.auth.changePassword('Old-password1!', 'New-password2!', 'New-password2!');
  assert.equal(calls[0].method, 'PUT');
  assert.equal(calls[0].url, '/api/auth/change-password');
  assert.equal(calls[0].headers.get('Authorization'), 'Bearer real-token');
  assert.deepEqual(JSON.parse(calls[0].body), { currentPassword: 'Old-password1!', newPassword: 'New-password2!', confirmPassword: 'New-password2!' });
  backend(() => ({ status: 400 }));
  await assert.rejects(modules.auth.changePassword('wrong', 'New-password2!', 'New-password2!'), error => error.status === 400);
});
test('shipping fee reads and updates the selected province including free shipping', async () => {
  backend(() => ({ provinceCode: '79', fee: 0 }));
  await modules.shipping.getFee('79');
  assert.equal(calls[0].url, '/api/shipping/provinces/79/fee');
  await modules.shipping.setFee('79', 0);
  assert.equal(calls[1].method, 'PUT');
  assert.equal(calls[1].url, '/api/shipping/provinces/79/fee');
  assert.deepEqual(JSON.parse(calls[1].body), { fee: 0 });
  assert.equal(calls[1].headers.get('Authorization'), 'Bearer real-token');
  await modules.shipping.setFee('01', 30000);
  assert.equal(calls[2].url, '/api/shipping/provinces/01/fee');
  assert.deepEqual(JSON.parse(calls[2].body), { fee: 30000 });
});
test('restoring a damaged unit requests IN_STOCK for the exact product and serial', async () => {
  backend(() => ({ productId, serialNumber: 'SN-1', quantity: 1 }));
  await modules.inventory.restoreToStock({ productId, serialNumber: 'SN-1', status: 'DAMAGED' });
  assert.equal(calls[0].url, `/api/inventory/${productId}/units/status`);
  assert.equal(calls[0].method, 'PATCH');
  assert.deepEqual(JSON.parse(calls[0].body), { serialNumber: 'SN-1', status: 'IN_STOCK' });
});
test('bulk stock import uploads an Excel file as multipart and exposes row errors', async () => {
  backend(() => ({ receivedQuantity: 2, products: [{ productId, receivedQuantity: 2, quantity: 5 }] }));
  const workbook = new File(['excel-data'], 'stock.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
  const result = await modules.inventory.importExcel(workbook);
  assert.equal(calls[0].url, '/api/inventory/import');
  assert.equal(calls[0].method, 'POST');
  assert.ok(calls[0].body instanceof FormData);
  assert.equal(calls[0].body.get('file').name, 'stock.xlsx');
  assert.equal(calls[0].headers.has('Content-Type'), false);
  assert.equal(result.receivedQuantity, 2);
  backend(() => ({ status: 400 }));
  await assert.rejects(modules.inventory.importExcel(workbook), error => error.status === 400 && !!error.errors);
});
test('admin product filters send price range, brand, category, status and sort to the backend', async () => {
  backend(() => ({ items: [], totalCount: 0, pageNumber: 1, pageSize: 20, brands: [], categories: [] }));
  await modules.product.getAdminProducts({ keyword: 'Laptop', categoryId: 'cat', brandId: 'brand', status: 'ACTIVE',
    minPrice: 0, maxPrice: 40000000, sortBy: 'rating', pageNumber: 1, pageSize: 20 });
  const url = new URL(calls[0].url, 'http://localhost');
  assert.equal(url.pathname, '/api/admin/products');
  assert.equal(url.searchParams.get('minPrice'), '0');
  assert.equal(url.searchParams.get('maxPrice'), '40000000');
  assert.equal(url.searchParams.get('sortBy'), 'rating');
  assert.equal(url.searchParams.get('categoryId'), 'cat');
  assert.equal(url.searchParams.get('brandId'), 'brand');
  assert.equal(url.searchParams.get('status'), 'ACTIVE');
  assert.equal(calls.length, 1);
});
test('customer products request exactly one server page with all filters and preserve server order', async () => {
  const row = (id, price) => ({ id, name: id, price, status: 'ACTIVE', brandId: 'brand', categoryId: 'cat', images: [], rating: 4.5, reviewsCount: 2 });
  backend(() => ({ items: [row('expensive', 200), row('cheap', 100)], totalCount: 30, pageNumber: 2, pageSize: 2, brands: [], categories: [] }));
  const page = await modules.product.getProductPage({ pageNumber: 2, pageSize: 2, categoryIds: 'cat', brandId: 'brand', minPrice: 0, maxPrice: 200, sortBy: 'price_asc', search: 'Laptop' });
  assert.equal(calls.length, 1);
  const params = new URL(calls[0].url, 'http://localhost').searchParams;
  assert.deepEqual(Object.fromEntries(params), { pageNumber: '2', pageSize: '2', keyword: 'Laptop', categoryId: 'cat', brandId: 'brand', minPrice: '0', maxPrice: '200', sortBy: 'price_asc' });
  assert.deepEqual(page.items.map(item => item.id), ['expensive', 'cheap']);
  assert.equal(page.totalCount, 30);
  assert.equal(page.items[0].rating, 4.5);
});
test('inventory filters units at the API and uses serial-based receive and damage operations', async () => {
  backend(() => ({ items: [], totalCount: 0 }));
  await modules.inventory.getUnits({ productId, keyword: 'SN&1', status: 'IN_STOCK', pageNumber: 2, pageSize: 20 });
  const url = new URL(calls[0].url, 'http://localhost');
  assert.equal(url.pathname, '/api/inventory/units');
  assert.equal(url.searchParams.get('keyword'), 'SN&1');
  assert.equal(url.searchParams.get('productId'), productId);
  assert.equal(url.searchParams.get('status'), 'IN_STOCK');
  assert.equal(url.searchParams.get('pageNumber'), '2');
  await modules.inventory.receive(productId, ['SN-1', 'SN-2']);
  assert.equal(calls[1].url, `/api/inventory/${productId}/receive`);
  assert.deepEqual(JSON.parse(calls[1].body), { serialNumbers: ['SN-1', 'SN-2'] });
  await modules.inventory.markDamaged({ productId, serialNumber: 'SN-1', status: 'IN_STOCK' });
  assert.equal(calls[2].method, 'PATCH');
  assert.equal(calls[2].url, `/api/inventory/${productId}/units/status`);
  assert.deepEqual(JSON.parse(calls[2].body), { serialNumber: 'SN-1', status: 'DAMAGED' });
});
const cart = { cartId: 'cart', items: [{ productId, quantity: 3, product: {
  name: 'Laptop', price: 15000000, status: 'ACTIVE', images: [{ imageUrl: '/product-images/a.png', isPrimary: true }]
} }] };

test('cart maps the nested product snapshot and preserves unavailable items', async () => {
  backend(() => ({ ...cart, items: [...cart.items, { productId: 'missing', quantity: 1, product: null }] }));
  const items = await modules.cart.getCart();
  assert.equal(items[0].productName, 'Laptop');
  assert.equal(items[0].price, 15000000);
  assert.equal(items[0].imageUrl, '/product-images/a.png');
  assert.equal(items[1].unavailable, true);
  assert.equal(calls[0].headers.get('Authorization'), 'Bearer real-token');
});
test('cart sends selected quantity, updates and clears only existing item IDs', async () => {
  backend(call => call.method === 'GET' ? cart : null);
  await modules.cart.addToCart(productId, 3);
  assert.deepEqual(JSON.parse(calls[0].body), { productId, quantity: 3 });
  await modules.cart.updateQuantity(productId, 4);
  assert.equal(calls[2].method, 'PUT');
  assert.deepEqual(JSON.parse(calls[2].body), { quantity: 4 });
  await modules.cart.clearCart();
  assert.equal(calls.at(-1).method, 'DELETE');
  assert.equal(calls.at(-1).url, `/api/cart/items/${productId}`);
});
test('API errors retain backend validation details; 401 clears private session', async () => {
  backend(() => ({ status: 401 }));
  await assert.rejects(modules.cart.getCart(), error => error.status === 401 && error.errors.quantity[0] === 'invalid');
  assert.equal(localStorage.getItem('token'), null);
});
test('categories use supported API operations and refuse an unsupported delete', async () => {
  backend(call => call.method === 'GET' ? [{ id: 'cat', name: 'Laptop' }] : { id: 'cat', name: 'New' });
  await modules.category.getCategories();
  await modules.category.createCategory({ name: 'New', description: 'not supported' });
  await modules.category.updateCategory('cat', { name: 'Updated' });
  assert.deepEqual(JSON.parse(calls[1].body), { name: 'New' });
  assert.equal(calls[2].url, '/api/admin/categories/cat');
  await assert.rejects(modules.category.deleteCategory('cat'), error => error.status === 501);
  assert.equal(calls.length, 3);
});
test('admin category pagination uses a separate endpoint from public lookups', async () => {
  backend(call => call.url.startsWith('/api/admin/categories')
    ? { items: [{ id: 'c', name: 'Laptop' }], totalCount: 35, pageNumber: 2, pageSize: 20 }
    : [{ id: 'c', name: 'Laptop' }, { id: 'other', name: 'Phone' }]);
  const page = await modules.category.getAdminCategories({ pageNumber: 2, pageSize: 20, keyword: '  lap  ' });
  const request = new URL(calls[0].url, 'http://localhost');
  assert.equal(request.pathname, '/api/admin/categories');
  assert.equal(request.searchParams.get('pageNumber'), '2');
  assert.equal(request.searchParams.get('pageSize'), '20');
  assert.equal(request.searchParams.get('keyword'), 'lap');
  assert.equal(page.totalCount, 35);
  assert.equal((await modules.category.getCategories()).length, 2);
  assert.equal(calls[1].url, '/api/categories');
});
test('address creation sends ward code and separately sets default', async () => {
  backend(call => call.method === 'POST' ? { id: 'address', isDefault: false } : null);
  await modules.address.createAddress({ province: 'HCM', ward: 'Ward', wardCode: '001', detail: '10 Street', isDefault: true });
  assert.deepEqual(JSON.parse(calls[0].body), { wardCode: '001', detail: '10 Street' });
  assert.equal(calls[1].url, '/api/shipping-address/address/default');
  assert.equal(calls[1].method, 'PUT');
});
test('addresses map display names separately from location codes', async () => {
  backend(() => ({ items: [{ id: 'a', provinceCode: '79', provinceName: 'HCM', wardCode: '001', wardName: 'Ward', detail: '10 Street', isDefault: true }] }));
  const [address] = await modules.address.getAddresses();
  assert.equal(address.province, 'HCM');
  assert.equal(address.wardCode, '001');
  await assert.rejects(modules.address.createAddress({ province: 'HCM', ward: 'Ward', detail: 'x', isDefault: false }), error => error.status === 400);
});
test('voucher list maps unlimited usage and create/deactivate use admin endpoints', async () => {
  const voucher = { id: 'v', code: 'SALE', type: 'FIXED', value: 1000, maxUsage: null, usedCount: 1,
    minOrderAmount: null, isActive: true, createdAt: null };
  backend(call => call.method === 'GET' ? { items: [voucher], totalCount: 1, pageNumber: 1, pageSize: 20 } : voucher);
  const page = await modules.voucher.getVouchers();
  assert.equal(page.items[0].maxUsage, Number.MAX_SAFE_INTEGER);
  await modules.voucher.createVoucher({ code: 'SALE', type: 'FIXED', value: 1000, maxUsage: 10, minOrderAmount: 0 });
  assert.equal(JSON.parse(calls[1].body).startDate, null);
  await modules.voucher.deleteVoucher('v');
  assert.equal(calls[2].method, 'DELETE');
});
test('profile uploads use multipart and payment methods come from the API', async () => {
  backend(call => call.url.includes('transaction-methods') ? [{ id: 'cod', name: 'Cash', type: 'CASH', code: null }] : null);
  await modules.user.updateProfile({ fullName: 'Test User', phone: '0123456789', dateOfBirth: '2000-01-01' });
  assert.equal(calls[0].url, '/api/user-profile');
  assert.ok(calls[0].body instanceof FormData);
  assert.equal(calls[0].body.get('FullName'), 'Test User');
  assert.equal(calls[0].headers.has('Content-Type'), false);
  const [method] = await modules.payment.getPaymentMethods();
  assert.equal(method.id, 'cod');
});
test('users map nested profiles and deduplicate accounts with multiple roles', async () => {
  const record = { id: 'u', email: 'test@example.com', status: 'ACTIVE', roles: ['Staff'], createdAt: '2026-01-01', profile: { fullName: 'Test', phone: '0123' } };
  backend(() => ({ items: [record], pageNumber: 1, pageSize: 100, totalCount: 1 }));
  const users = await modules.user.getUsers();
  assert.equal(users.length, 1);
  assert.equal(users[0].fullName, 'Test');
});
test('user role selection is sent to the database-backed API without fetching other user types', async () => {
  backend(() => ({ items: [], totalCount: 0, pageNumber: 1, pageSize: 100 }));
  await modules.user.getUsers('Technical Admin');
  assert.equal(calls.length, 1);
  const request = new URL(calls[0].url, 'http://localhost');
  assert.equal(request.pathname, '/api/admin/users/staff');
  assert.equal(request.searchParams.get('role'), 'Technical Admin');
  await modules.user.getUsers('Customer');
  assert.ok(calls[1].url.startsWith('/api/admin/users/customers?'));
});
test('editing an account resolves a role ID from the public role-list response', async () => {
  const record = { id: 'u', email: 'test@example.com', status: 'ACTIVE', roles: ['Staff'], createdAt: '', profile: { fullName: '', phone: '' } };
  backend(call => {
    if (call.url === '/api/admin/rbac/roles') return [{ id: 'business-role', name: 'Business Admin' }];
    if (call.method === 'PUT') return null;
    return { items: [record], pageNumber: 1, pageSize: 100, totalCount: 1 };
  });
  await modules.user.updateUser('u', { role: 'Business Admin' });
  const assignment = calls.find(call => call.url === '/api/admin/rbac/users/u/roles');
  assert.deepEqual(JSON.parse(assignment.body), { roleIds: ['business-role'] });
});
test('changing payment method uses the replace API without mutating mock orders', async () => {
  backend(() => ({ paymentId: 'new-payment' }));
  await modules.order.updatePaymentMethod(productId, 'method-id');
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/payments/replace');
  assert.deepEqual(JSON.parse(calls[0].body), { type: 'ORDER', refId: productId, transactionMethodId: 'method-id' });
});
test('inventory sends serials, and order creation cannot clear the real cart', async () => {
  backend(() => null);
  await modules.product.updateInventory(productId, ['SN-1', 'SN-2']);
  assert.equal(calls[0].url, `/api/inventory/${productId}/receive`);
  assert.deepEqual(JSON.parse(calls[0].body), { serialNumbers: ['SN-1', 'SN-2'] });
  await assert.rejects(modules.order.createOrder({}), error => error.status === 501);
  assert.equal(calls.length, 1);
});
test('payment retries query a real order then request the checkout URL', async () => {
  backend(call => call.method === 'GET' ? { items: [{ id: 'payment', status: 'PENDING' }] } : { paymentUrl: 'https://payment.example/checkout' });
  assert.equal(await modules.order.repay(productId), 'https://payment.example/checkout');
  assert.equal(calls[1].url, '/api/payments/payment/checkout');
  await assert.rejects(modules.order.repay('ORD-100'), error => error.status === 400);
});

test('unimplemented features reject reads and writes instead of using RAM records', async () => {
  backend(() => { throw new Error('Unexpected HTTP request'); });
  const operations = [
    () => modules.order.getOrders(),
    () => modules.order.getOrderById('example'),
    () => modules.order.updateOrderStatus('example', 'COMPLETED'),
    () => modules.return.getReturnRequests(),
    () => modules.return.createReturnRequest({}),
    () => modules.repair.getRepairRequests(),
    () => modules.repair.createRepairRequest({}),
    () => modules.review.getReviewsByProductId(productId),
    () => modules.review.submitReview({}),
    () => modules.notification.getNotifications(),
    () => modules.notification.markAllAsRead(),
    () => modules.dashboard.getSalesStats(),
    () => modules.dashboard.getReportSummary(),
  ];
  for (const operation of operations) {
    await assert.rejects(operation, error => error.status === 501 && error.message.includes('API'));
  }
  assert.equal(calls.length, 0);
});
