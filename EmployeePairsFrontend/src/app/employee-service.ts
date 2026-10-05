import { HttpClient } from '@angular/common/http';
import { Service, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { EmployeePair } from './employee-pair';

@Service()
export class EmployeeService {
  private readonly http = inject(HttpClient);

  // Proxied to the backend by the dev server (see proxy.conf.json).
  private readonly apiUrl = '/api/Employees/longest-working-pair';

  /**
   * Uploads the CSV file and returns the longest working pair,
   * or null when no two employees ever overlapped (HTTP 204).
   */
  findLongestWorkingPair(file: File): Observable<EmployeePair | null> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http
      .post<EmployeePair>(this.apiUrl, formData, { observe: 'response' })
      .pipe(map((response) => (response.status === 204 ? null : response.body)));
  }
}
