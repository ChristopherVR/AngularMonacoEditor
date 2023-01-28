import { localCacheProvider, sessionCacheProvider } from './providers/browser.cache.providers';
import { CacheProviderType } from './cache.provider.type';
import { memoryCacheProvider } from './providers/memory.cache.provider';

export const cacheProviderFactory = <T = unknown>() => ({
  // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
  ofType(type: CacheProviderType) {
    switch (type) {
      case CacheProviderType.Memory:
        return memoryCacheProvider<T>();

      case CacheProviderType.Persistent:
        return localCacheProvider<T>();

      case CacheProviderType.Session:
        return sessionCacheProvider<T>();
    }
  },
});
