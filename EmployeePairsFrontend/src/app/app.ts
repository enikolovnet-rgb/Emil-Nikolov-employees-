import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { EmployeeService } from './employee-service';
import { EmployeePair } from './employee-pair';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly employeeService = inject(EmployeeService);

  protected readonly fileName = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly pair = signal<EmployeePair | null>(null);
  protected readonly noOverlap = signal(false);

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    // Reset so selecting the same file again still fires a change event.
    input.value = '';
    if (!file) {
      return;
    }

     if (!file.name.toLowerCase().endsWith('.csv')) {
      this.error.set('Only .csv files are supported.');
      return;
     }

    this.fileName.set(file.name);
    this.loading.set(true);
    this.error.set(null);
    this.pair.set(null);
    this.noOverlap.set(false);

    this.employeeService.findLongestWorkingPair(file).subscribe({
      next: (pair) => {
        this.pair.set(pair);
        this.noOverlap.set(pair === null);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(this.describeError(err));
        this.loading.set(false);
      }
    });
  }

  private describeError(err: HttpErrorResponse): string {
    if (err.status === 0) {
      return 'Could not reach the server. Make sure the API is running.';
    }
    const problem = err.error;
    return problem?.detail ?? problem?.title ?? `Request failed with status ${err.status}.`;
  }
}
