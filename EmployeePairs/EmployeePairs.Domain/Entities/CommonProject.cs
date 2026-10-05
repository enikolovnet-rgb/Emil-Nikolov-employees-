namespace EmployeePairs.Domain.Entities
{
    public record CommonProject(int EmployeeId1, int EmployeeId2, int ProjectId)
    {
        public int DaysWorked { get; set; }
    }
}
