namespace EmployeePairs.Domain.Entities
{
    public record EmployeeProjectAssignment(int EmployeeId, int ProjectId, DateOnly Begining, DateOnly End);
}
