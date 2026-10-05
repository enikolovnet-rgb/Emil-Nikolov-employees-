namespace EmployeePairs.Domain.Entities
{
    public record EmployeePair(int EmployeeId1, int EmployeeId2, IReadOnlyList<CommonProject> CommonProjects)
    {
        public int TotalDaysWorked => CommonProjects.Sum(p => p.DaysWorked);
    }
}
