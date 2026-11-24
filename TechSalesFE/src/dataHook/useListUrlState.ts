import { useRef } from 'react';
import { useSearchParams } from 'react-router';

export function readPageNumber(value: string | null, fallback: number, maximum: number): number {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed >= 1 && parsed <= maximum ? parsed : fallback;
}

export function useListUrlState<T extends Record<string, string>>(defaults: T, prefix = '') {
  const [params, setParams] = useSearchParams();
  const latest = useRef(params);
  latest.current = params;
  const key = (name: string) => `${prefix}${name}`;
  const update = (changes: Record<string, string | null>, replace = false) => {
    const next = new URLSearchParams(latest.current);
    Object.entries(changes).forEach(([name, value]) => {
      if (value === null) next.delete(key(name));
      else next.set(key(name), value);
    });
    latest.current = next;
    setParams(next, { replace });
  };
  return {
    pageNumber: readPageNumber(params.get(key('page')), 1, 1_000_000),
    pageSize: readPageNumber(params.get(key('pageSize')), 20, 100),
    setPageNumber: (page: number) => update({ page: String(page) }),
    setPageSize: (size: number) => update({ pageSize: String(size), page: '1' }),
    field: <K extends keyof T & string>(name: K): [T[K], (value: T[K]) => void] => [
      (params.get(key(name)) ?? defaults[name]) as T[K],
      value => update({ [name]: value === defaults[name] ? null : value, page: '1' }, true),
    ],
  };
}
