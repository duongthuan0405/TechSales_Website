import test from 'node:test';
import assert from 'node:assert/strict';
import { build } from 'esbuild';

const modules = {};
for (const name of ['cart', 'category', 'address', 'voucher', 'user', 'payment', 'product', 'order']) {
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
