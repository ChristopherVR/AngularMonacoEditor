/* eslint-disable prefer-arrow/prefer-arrow-functions */
import { of } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { CacheProvider, CachePayload } from '../types';

type WebStorage = {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
};

const dynamicKeyPrex = '___dynamic___';
export const storageCacheProvider = <T>(storage: WebStorage): CacheProvider<T> => ({
  get(key: string) {
    try {
      const payload = storage.getItem(`${dynamicKeyPrex}${key}`);

      if (!payload) {
        return;
      }

      const parsed = JSON.parse(payload);

      return {
        ...parsed,
        value: of(parsed.value),
      };
    } catch (e) {
      return;
    }
  },
  set(key: string, payload: CachePayload<T>) {
    payload.value.pipe(take(1), filter(Boolean)).subscribe((value) => {
      storage.setItem(
        `${dynamicKeyPrex}${key}`,
        JSON.stringify({
          ...payload,
          value,
        }),
      );
    });
  },
  unset(key: string) {
    storage.removeItem(`${key}`);
  },
  getAll() {
    const values: (string | null)[] = [];

    Object.keys(storage).forEach((y) => {
      if (y.includes(dynamicKeyPrex)) {
        values.push(storage.getItem(y));
      }
    });

    return values;
  },
});
