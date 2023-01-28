import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { CacheProviderType } from './cache.provider.type';
import { cacheProviderFactory } from './provider.factory';

/**
 * Calc hash of string
 * Source: https://stackoverflow.com/a/7616484
 *
 * @param s
 * @returns Hashcode for the given string value.
 */
const hashCode = (s: string): string => {
  let hash = 0;
  let chr: number;
  if (s.length === 0) {
    return hash.toString(36);
  }
  for (let i = 0; i < s.length; i++) {
    chr = s.charCodeAt(i);
    // eslint-disable-next-line no-bitwise
    hash = (hash << 5) - hash + chr;
    // eslint-disable-next-line no-bitwise
    hash |= 0;
  }
  return hash.toString(36);
};

/**
 * Cache Observable function for ms milliseconds
 *
 * @param {number} ms
 */

export const getOrSetCacheValue = <TData>(
  ms: number = 100_0000, // 100 seconds
  prefix: string,
  args: object,
  observ$: Observable<TData>,
  cacheProvideType = CacheProviderType.Memory,
) => {
  const cacheFactory = cacheProviderFactory<TData>();
  const cache = cacheFactory.ofType(cacheProvideType);
  const key = hashCode(`${prefix}:${JSON.stringify(args)}`);
  const cached = cache.get(key);

  if (cached) {
    if (cached.expiry !== undefined && isKeyExpired(cached.expiry)) {
      cache.unset(key);
    } else {
      return cached.value;
    }
  }

  const value = observ$.pipe(
    shareReplay({
      bufferSize: 1,
      refCount: true,
    }),
  );

  const lastUpdated = new Date().getTime();

  const expiry = lastUpdated + ms;

  // caching value
  cache.set(key, {
    value,
    expiry,
    lastUpdated,
  });

  return value;
};

const isKeyExpired = (expirationDate: number) => {
  const currentTimestamp = new Date().getTime();

  return currentTimestamp >= expirationDate;
};
