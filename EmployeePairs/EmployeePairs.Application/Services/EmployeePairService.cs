using EmployeePairs.Application.Exceptions;
using EmployeePairs.Domain.Entities;
using System.Globalization;
using System.Text;

namespace EmployeePairs.Application.Services
{
    internal class EmployeePairService : IEmployeePairService
    {
        private const int ExpectedFieldCount = 4;

        private readonly TimeProvider _timeProvider;

        public EmployeePairService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public async Task<EmployeePair?> FindLongestWorkingPairAsync(Stream content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

            List<EmployeeProjectAssignment> assignments = new List<EmployeeProjectAssignment>();

            using (var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
            {
                string line;
                bool isHeader = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] columns = line.Split(',').Select(s => s.Trim()).ToArray();

                    assignments.Add(ParseAssignment(columns, lineNumber: assignments.Count + 1, today: DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime)));
                }
            }

            if (assignments.Count == 0)
            {
                throw new InvalidInputFileException("The file does not contain any employee records.");
            }

            return EmployeeOverlapService.CalculateExactOverlapDays(assignments);
        }

        private static EmployeeProjectAssignment ParseAssignment(string[] fields, int lineNumber, DateOnly today)
        {
            if (fields.Length != ExpectedFieldCount)
            {
                throw new InvalidInputFileException(lineNumber,
                    $"Expected {ExpectedFieldCount} values (EmpID, ProjectID, DateFrom, DateTo) but found {fields.Length}.");
            }

            var employeeId = ParseId(fields[0], "EmpID", lineNumber);
            var projectId = ParseId(fields[1], "ProjectID", lineNumber);

            if (!FlexibleDateParser.TryParse(fields[2], out var dateFrom))
            {
                throw new InvalidInputFileException(lineNumber, $"DateFrom '{fields[2]}' is not a valid date.");
            }

            var dateTo = today;
            if (!IsNull(fields[3]) && !FlexibleDateParser.TryParse(fields[3], out dateTo))
            {
                throw new InvalidInputFileException(lineNumber, $"DateTo '{fields[3]}' is not a valid date.");
            }

            if (dateTo < dateFrom)
            {
                throw new InvalidInputFileException(lineNumber,
                    $"DateTo {dateTo:yyyy-MM-dd} is before DateFrom {dateFrom:yyyy-MM-dd}.");
            }

            return new EmployeeProjectAssignment(employeeId, projectId, dateFrom, dateTo);
        }

        private static int ParseId(string value, string fieldName, int lineNumber)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                throw new InvalidInputFileException(lineNumber, $"{fieldName} '{value}' is not a valid integer.");
            }

            return id;
        }

        private static bool IsNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.Equals("NULL", StringComparison.OrdinalIgnoreCase);
        }
    }
}
