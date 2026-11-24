import test from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { build } from 'esbuild';

const bundle = await build({
  stdin: { contents: `
    import React from 'react';
    import { renderToString } from 'react-dom/server';
    import { useListUrlState } from './src/dataHook/useListUrlState';
    export function restore(prefix = '') {
      let state;
      function Probe() { state = useListUrlState({ keyword: '', role: 'all' }, prefix); return null; }
      renderToString(<Probe />);
      return state;
    }`, loader: 'tsx', resolveDir: process.cwd() },
  bundle: true, write: false, platform: 'node', format: 'cjs', jsx: 'automatic',
  plugins: [{ name: 'url-navigation', setup(builder) {
    builder.onResolve({ filter: /^react-router$/ }, () => ({ path: 'router', namespace: 'fixture' }));
    builder.onLoad({ filter: /.*/, namespace: 'fixture' }, () => ({ contents: `
      export const useSearchParams = () => [new URLSearchParams(globalThis.__listUrl),
        next => { globalThis.__listUrl = next.toString(); }];` }));
  } }],
});
const compiled = { exports: {} };
new Function('require', 'module', 'exports', bundle.outputFiles[0].text)(createRequire(import.meta.url), compiled, compiled.exports);
const { restore } = compiled.exports;

test('reload restores page, size and keyword from URL; invalid numbers fall back safely', () => {
  globalThis.__listUrl = 'page=3&pageSize=50&keyword=090123';
  const state = restore();
  assert.equal(state.pageNumber, 3);
  assert.equal(state.pageSize, 50);
  assert.equal(state.field('keyword')[0], '090123');
  globalThis.__listUrl = 'page=-1&pageSize=1000';
  assert.equal(restore().pageNumber, 1);
  assert.equal(restore().pageSize, 20);
});

test('page changes preserve filters and other tabs; changing filters and size reset only the active page', () => {
  globalThis.__listUrl = 'tab=brands&brands.page=4&brands.pageSize=10&categories.page=7&stockProduct=abc';
  let state = restore('brands.');
  state.setPageNumber(5);
  state.setPageSize(50);
  state.field('keyword')[1]('Sony');
  const params = new URLSearchParams(globalThis.__listUrl);
  assert.equal(params.get('brands.page'), '1');
  assert.equal(params.get('brands.pageSize'), '50');
  assert.equal(params.get('categories.page'), '7');
  assert.equal(params.get('tab'), 'brands');
  assert.equal(params.get('stockProduct'), 'abc');
  state = restore('brands.');
  assert.equal(state.field('keyword')[0], 'Sony');
  assert.equal(state.pageSize, 50);
});
