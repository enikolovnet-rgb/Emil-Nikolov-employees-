namespace EmployeePairs.Application.Exceptions
{
    public class InvalidInputFileException : Exception
    {
        public InvalidInputFileException(string message)
            : base(message)
        {
        }

        public InvalidInputFileException(int lineNumber, string message)
            : base($"Line {lineNumber}: {message}")
        {
            LineNumber = lineNumber;
        }

        public int? LineNumber { get; }
    }

}
