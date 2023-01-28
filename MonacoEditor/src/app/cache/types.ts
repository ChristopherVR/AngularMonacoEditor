import { Observable } from 'rxjs';

export interface CachePayload<T> {
  value: Observable<T>;
  expiry?: number;
  lastUpdated: number;
}

export interface CacheProvider<T> {
  getAll(): CachePayload<unknown>[] | (string | null)[] | undefined;
  get(key: string): CachePayload<T> | undefined;
  set(key: string, value: CachePayload<T>): void;
  unset(key: string): void;
}

export type ArgProps = string | Date | boolean | number | object | Record<string, unknown>;
