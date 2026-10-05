using EmployeePairs.Domain.Entities;

namespace EmployeePairs.Api.Contracts
{
    public record EmployeePairResponse(
    int EmployeeId1,
    int EmployeeId2,
    int TotalDaysWorked,
    IReadOnlyList<CommonProjectResponse> CommonProjects)
    {
        public static EmployeePairResponse FromDomain(EmployeePair pair) => new(
            pair.EmployeeId1,
            pair.EmployeeId2,
            pair.TotalDaysWorked,
            pair.CommonProjects
                .Select(p => new CommonProjectResponse(p.EmployeeId1, p.EmployeeId2, p.ProjectId, p.DaysWorked))
                .ToList());
    }

    public record CommonProjectResponse(int EmployeeId1, int EmployeeId2, int ProjectId, int DaysWorked);

}
