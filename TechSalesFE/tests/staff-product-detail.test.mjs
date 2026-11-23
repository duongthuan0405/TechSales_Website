import test from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { build } from 'esbuild';

const result = await build({
  stdin: { contents: `
    import React from 'react';
    import { renderToString } from 'react-dom/server';
    import { MemoryRouter, Routes, Route } from 'react-router';
    import { StaffProductDetailPage } from './src/app/pages/sales/StaffProductDetailPage';
    export function render() {
      return renderToString(<MemoryRouter initialEntries={['/business/products/test-id']}>
        <Routes><Route path="/business/products/:id" element={<StaffProductDetailPage />} /></Routes>
      </MemoryRouter>);
    }`, loader: 'tsx', resolveDir: process.cwd() },
  bundle: true, write: false, format: 'cjs', platform: 'node', jsx: 'automatic',
  define: { 'process.env.NODE_ENV': '"production"' },
  plugins: [{ name: 'isolated-api-fixtures', setup(builder) {
    builder.onLoad({ filter: /productDataHook\.ts$/ }, () => ({ contents:
      'export const useGetProduct = () => globalThis.__productDetailFixture;', loader: 'ts' }));
    builder.onLoad({ filter: /reviewDataHook\.ts$/ }, () => ({ contents: `
      export const useGetProductReviews = () => ({ data: { reviews: [] }, isLoading: false });
      export const useModerateReview = () => ({ mutate() {} });
      export const useReplyToReview = () => ({ mutate() {} });`, loader: 'ts' }));
  } }],
});
const compiled = { exports: {} };
new Function('require', 'module', 'exports', result.outputFiles[0].text)(createRequire(import.meta.url), compiled, compiled.exports);

test('business product details render price and stock without a runtime ReferenceError', () => {
  globalThis.__productDetailFixture = { data: { id: 'test-id', name: 'Test Laptop', description: 'Details',
    price: 15000000, status: 'ACTIVE', stock: 7 }, isLoading: false, refetch() {} };
  const html = compiled.exports.render();
  assert.ok(html.includes('Test Laptop'));
  assert.ok(html.includes('15.000.000'));
  assert.ok(html.includes('Đang kinh doanh'));
  assert.ok(html.includes('sản phẩm khả dụng'));
});
test('failed product requests render an error and retry instead of a blank page', () => {
  globalThis.__productDetailFixture = { data: undefined, isLoading: false,
    error: new Error('Không tải được sản phẩm.'), refetch() {} };
  const html = compiled.exports.render();
  assert.ok(html.includes('role="alert"'));
  assert.ok(html.includes('Không tải được sản phẩm.'));
  assert.ok(html.includes('Thử lại'));
});
