using System.Globalization;

namespace EmployeePairs.Application
{
    public static class FlexibleDateParser
    {
        private static readonly string[] Formats =
    [
        // Year first
        "yyyy-M-d", "yyyy/M/d", "yyyy.M.d", "yyyyMMdd",
        "yyyy-M-d HH:mm", "yyyy-M-d HH:mm:ss", "yyyy/M/d HH:mm", "yyyy/M/d HH:mm:ss",
        "yyyy-M-dTHH:mm", "yyyy-M-dTHH:mm:ss", "yyyy-M-dTHH:mm:ss.FFFFFFF", "yyyy-M-dTHH:mm:ssK", "yyyy-M-dTHH:mm:ss.FFFFFFFK",

        // Day first
        "d-M-yyyy", "d/M/yyyy", "d.M.yyyy",
        "d/M/yyyy HH:mm", "d/M/yyyy HH:mm:ss", "d.M.yyyy HH:mm", "d.M.yyyy HH:mm:ss",
        "d MMM yyyy", "d MMMM yyyy", "d-MMM-yyyy", "d-MMMM-yyyy", "d MMM, yyyy", "d-MMM-yy",

        // Month first
        "M/d/yyyy", "M-d-yyyy", "M/d/yyyy h:mm tt", "M/d/yyyy h:mm:ss tt", "M/d/yyyy HH:mm", "M/d/yyyy HH:mm:ss",
        "MMM d yyyy", "MMMM d yyyy", "MMM d, yyyy", "MMMM d, yyyy",

        // With weekday
        "ddd, d MMM yyyy", "dddd, d MMMM yyyy", "dddd, MMMM d, yyyy",
    ];

        public static bool TryParse(string? value, out DateOnly date)
        {
            date = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var text = value.Trim();

            if (DateTime.TryParseExact(text, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var exact))
            {
                date = DateOnly.FromDateTime(exact);
                return true;
            }

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed))
            {
                date = DateOnly.FromDateTime(parsed);
                return true;
            }

            return false;
        }
    }
}
