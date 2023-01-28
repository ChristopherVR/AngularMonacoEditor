import { CacheProvider } from '../types';
import { memoryCacheProvider } from './memory.cache.provider';
import { storageCacheProvider } from './web.storage.cache.provider';

export const sessionCacheProvider = <T>(): CacheProvider<T> => {
  const storage: Storage = window.sessionStorage;

  return storage ? storageCacheProvider(storage) : memoryCacheProvider();
};

export const localCacheProvider = <T>(): CacheProvider<T> => {
  const storage: Storage = window.localStorage;

  return storage ? storageCacheProvider(storage) : memoryCacheProvider();
};
