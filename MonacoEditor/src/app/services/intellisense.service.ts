import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MonacoRequest, SignatureResponse, HoverInfo, CodeCheck, TabCompletion } from '../base-editor/interfaces';
import { getOrSetCacheValue } from '../cache/cache-observable.decorator';

type CachePayloadRequest = MonacoRequest & {
  type: 'signature' | 'tab' | 'check' | 'hover';
};

@Injectable({
  providedIn: 'root',
})
export class IntellisenseService {
  constructor(private client: HttpClient) {}

  /** Returns the method signature information. */
  public validateSignatureInformation(payload: MonacoRequest, id: string) {
    const url = `${environment.apiEndpoint}/dynamicModule/signature`;
    return this.setAndGetResponse<SignatureResponse>(
      url,
      {
        ...payload,
        type: 'signature',
      },
      id,
    );
  }
  /** Returns the code hover information */
  public validateHoverInformation(payload: MonacoRequest, id: string) {
    const url = `${environment.apiEndpoint}/dynamicModule/hover`;
    return this.setAndGetResponse<HoverInfo>(
      url,
      {
        ...payload,
        type: 'hover',
      },
      id,
    );
  }

  /** Validates whether the code can compile successfully. */
  public analyseCode(payload: MonacoRequest, id: string) {
    const url = `${environment.apiEndpoint}/dynamicModule/check`;
    return this.setAndGetResponse<CodeCheck[]>(
      url,
      {
        ...payload,
        type: 'check',
      },
      id,
    );
  }

  /** Provides a list of tab suggestions */
  public valdiateTabCodeCompletion(payload: MonacoRequest, id: string) {
    const url = `${environment.apiEndpoint}/dynamicModule/tab`;
    return this.setAndGetResponse<TabCompletion[]>(
      url,
      {
        ...payload,
        type: 'tab',
      },
      id,
    );
  }

  private setAndGetResponse<TData>(url: string, payload: CachePayloadRequest, id: string) {
    const value = getOrSetCacheValue<TData>(
      100_000,
      id,
      payload,
      this.client.post<TData>(url, {
        position: payload.position,
      }),
    );

    return value;
  }
}
