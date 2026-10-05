# Employee Pairs

Finds the pair of employees who have worked together on common projects for the longest period of time.

The solution has two parts:

| Folder | Description |
|---|---|
| `EmployeePairs/` | ASP.NET Core Web API (.NET 10) that parses the CSV and calculates the result |
| `EmployeePairsFrontend/` | Angular 22 UI with a file picker and a data grid of the pair's common projects |

![UI screenshot](docs/screenshot.png)

## Input format

A CSV file with a header row and the columns:

```
EmpID, ProjectID, DateFrom, DateTo
143, 12, 2013-11-01, 2014-01-05
218, 10, 2012-05-16, NULL
143, 10, 2009-01-01, 2011-04-27
```

- `DateTo` can be `NULL` (or empty), which means **today**.
- Whitespace around values is ignored.
- The file is read as UTF-8, with or without a BOM.

## Output

The longest-working pair, the total days they worked together, and a breakdown per common project:

```json
{
  "employeeId1": 101,
  "employeeId2": 102,
  "totalDaysWorked": 456,
  "commonProjects": [
    { "employeeId1": 101, "employeeId2": 102, "projectId": 1, "daysWorked": 214 },
    { "employeeId1": 101, "employeeId2": 102, "projectId": 2, "daysWorked": 122 },
    { "employeeId1": 101, "employeeId2": 102, "projectId": 3, "daysWorked": 120 }
  ]
}
```

The UI shows the summary as `101, 102, 456` and lists the common projects in a grid with the columns **Employee ID #1, Employee ID #2, Project ID, Days worked**.

## Running locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS) and npm

### 1. Start the API

```bash
cd EmployeePairs/EmployeePairs.Api
dotnet run --launch-profile https
```

The API runs at `https://localhost:7097`. In Development, interactive API docs (Scalar) are available at `https://localhost:7097/scalar`.

### 2. Start the UI

```bash
cd EmployeePairsFrontend
npm install
npm start
```

Open `http://localhost:4200`. The Angular dev server proxies `/api` requests to the API (see `proxy.conf.json`), so no CORS setup is needed.

### 3. Try it

Choose `samples/employees-test.csv` in the UI. The expected result is `101, 102, 456`.

## API

### `POST /api/Employees/longest-working-pair`

`multipart/form-data` with a single field named `file`.

| Status | Meaning |
|---|---|
| `200 OK` | The longest-working pair with its common projects |
| `204 No Content` | The file is valid, but no two employees ever overlapped on a project |
| `400 Bad Request` | Invalid input, returned as [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) |

Example error:

```json
{
  "title": "Invalid input file",
  "status": 400,
  "detail": "Line 7: DateTo '31.13.2020' is not a valid date.",
  "traceId": "0HN7..."
}
```

```bash
curl -k -F "file=@samples/employees-test.csv" https://localhost:7097/api/Employees/longest-working-pair
```

## How it works

1. **Parse.** Each row becomes an assignment `(EmpID, ProjectID, DateFrom, DateTo)`. `NULL` is resolved to today, and invalid rows fail with a line-level error.
2. **Merge.** On each project, an employee's own periods are merged first, so duplicated or overlapping rows for the same employee aren't counted twice.
3. **Overlap.** For every pair of employees on the same project, the overlapping days of their periods are calculated.
4. **Aggregate.** Overlaps are summed per pair across all common projects, and the pair with the highest total wins.

## Assumptions

- **Days are counted inclusively.** Both the start and the end date count as worked days, so two employees who both worked on 2015-01-01 only share 1 day.
- **Consecutive periods don't overlap.** If one employee ends on 2022-06-30 and the other starts on 2022-07-01, they share 0 days.
- **"Today" is the server's current UTC date** when `DateTo` is `NULL`.
- **Ties:** if several pairs share the highest total, the first one found is returned.
- **The first line is always treated as a header** and skipped.
- **The UI shows only the winning pair's projects,** as the task describes. The calculation could also return the projects of every pair if needed.

## Supported date formats

Dates are parsed with `InvariantCulture`, first against an explicit list of formats and then with a general fallback. Supported formats include:

| Style | Examples |
|---|---|
| Year first | `2013-11-01`, `2013/11/01`, `2013.11.01`, `20131101`, ISO 8601 with time |
| Day first | `01.11.2013`, `01-11-2013`, `1 Nov 2013`, `1 November 2013`, `01-Nov-13` |
| Month first | `11/01/2013`, `Nov 1, 2013`, `November 1 2013` |
| With weekday | `Fri, 1 Nov 2013`, `Friday, November 1, 2013` |

> **Ambiguity:** with slashes, day-first (`d/M/yyyy`) is tried before month-first (`M/d/yyyy`), so `01/11/2013` is read as **1 November 2013**. Use an unambiguous format like `yyyy-MM-dd` when possible.

## Project structure

```
EmployeePairs/
├── EmployeePairs.Api            Controller, response contracts, global exception handler, OpenAPI/Scalar
├── EmployeePairs.Application    CSV parsing, flexible date parsing, overlap calculation
├── EmployeePairs.Domain         EmployeeProjectAssignment, CommonProject, EmployeePair
└── EmployeePairs.UnitTests      NUnit + Moq tests

EmployeePairsFrontend/
└── src/app                      Upload component, EmployeeService, models
```

## Tests

```bash
cd EmployeePairs
dotnet test
```

The tests cover the per-project breakdown, the no-overlap case, single-day overlaps, duplicate rows, `NULL` resolved to a mocked "today" (via `TimeProvider`), mixed date formats, and a header-only file.

## Docker

The API includes a Dockerfile:

```bash
cd EmployeePairs
docker build -f EmployeePairs.Api/Dockerfile -t employee-pairs-api .
docker run -p 8080:8080 employee-pairs-api
```
