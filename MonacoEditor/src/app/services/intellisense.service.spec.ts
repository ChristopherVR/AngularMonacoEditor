import { TestBed } from '@angular/core/testing';

import { IntellisenseService } from './intellisense.service';

describe('IntellisenseService', () => {
  let service: IntellisenseService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IntellisenseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
