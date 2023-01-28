/* eslint-disable prefer-arrow/prefer-arrow-functions */

import { CachePayload, CacheProvider } from '../types';

// global cache
const cache = new Map<string, CachePayload<unknown>>();

export const memoryCacheProvider = <T>(): CacheProvider<T> => ({
  get(key: string) {
    return cache.get(key) as CachePayload<T> | undefined;
  },
  set(key: string, value: CachePayload<T>) {
    cache.set(key, value);
  },
  unset(key: string) {
    cache.delete(key);
  },
  getAll() {
    return Array.from(cache.values());
  },
});
