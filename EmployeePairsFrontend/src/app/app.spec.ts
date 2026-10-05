import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the common projects in a datagrid after a file is selected', async () => {
    const fixture = TestBed.createComponent(App);
    const httpMock = TestBed.inject(HttpTestingController);
    await fixture.whenStable();

    const input = fixture.nativeElement.querySelector('input[type=file]') as HTMLInputElement;
    const file = new File(['csv'], 'data.csv', { type: 'text/csv' });
    Object.defineProperty(input, 'files', { value: [file] });
    input.dispatchEvent(new Event('change'));

    httpMock.expectOne('/api/Employees/longest-working-pair').flush({
      employeeId1: 143,
      employeeId2: 218,
      totalDaysWorked: 8,
      commonProjects: [
        { employeeId1: 143, employeeId2: 218, projectId: 10, daysWorked: 5 },
        { employeeId1: 143, employeeId2: 218, projectId: 12, daysWorked: 3 }
      ]
    });
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const headers = Array.from(compiled.querySelectorAll('th')).map((th) => th.textContent?.trim());
    expect(headers).toEqual(['Employee ID #1', 'Employee ID #2', 'Project ID', 'Days worked']);
    expect(compiled.querySelectorAll('tbody tr').length).toBe(2);
    expect(compiled.textContent).toContain('143, 218, 8');
  });

  it('should show the server error detail when the upload fails', async () => {
    const fixture = TestBed.createComponent(App);
    const httpMock = TestBed.inject(HttpTestingController);
    await fixture.whenStable();

    const input = fixture.nativeElement.querySelector('input[type=file]') as HTMLInputElement;
    Object.defineProperty(input, 'files', { value: [new File(['x'], 'bad.csv')] });
    input.dispatchEvent(new Event('change'));

    httpMock
      .expectOne('/api/Employees/longest-working-pair')
      .flush({ title: 'Invalid input file', detail: 'Line 1 is malformed.' }, { status: 400, statusText: 'Bad Request' });
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('[role=alert]').textContent).toContain('Line 1 is malformed.');
  });
});
