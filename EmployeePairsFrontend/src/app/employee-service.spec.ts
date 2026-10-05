import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { EmployeeService } from './employee-service';
import { EmployeePair } from './employee-pair';

describe('EmployeeService', () => {
  let service: EmployeeService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(EmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should upload the file as multipart form data and return the pair', () => {
    const file = new File(['143, 12, 2013-11-01, 2014-01-05'], 'data.csv', { type: 'text/csv' });
    const pair: EmployeePair = {
      employeeId1: 143,
      employeeId2: 218,
      totalDaysWorked: 8,
      commonProjects: [{ employeeId1: 143, employeeId2: 218, projectId: 10, daysWorked: 8 }]
    };
    let result: EmployeePair | null | undefined;

    service.findLongestWorkingPair(file).subscribe((r) => (result = r));

    const req = httpMock.expectOne('/api/Employees/longest-working-pair');
    expect(req.request.method).toBe('POST');
    expect((req.request.body as FormData).get('file')).toBeInstanceOf(File);
    req.flush(pair);

    expect(result).toEqual(pair);
  });

  it('should return null when the server responds with 204 No Content', () => {
    const file = new File([''], 'data.csv');
    let result: EmployeePair | null | undefined;

    service.findLongestWorkingPair(file).subscribe((r) => (result = r));

    httpMock
      .expectOne('/api/Employees/longest-working-pair')
      .flush(null, { status: 204, statusText: 'No Content' });

    expect(result).toBeNull();
  });
});
