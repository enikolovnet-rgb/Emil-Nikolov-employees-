using EmployeePairs.Domain.Entities;

public static class EmployeeOverlapService
{
    public static EmployeePair? CalculateExactOverlapDays(IEnumerable<EmployeeProjectAssignment> assignments)
    {
        var commonProjects = new List<CommonProject>();

        foreach (var projectGroup in assignments.GroupBy(a => a.ProjectId))
        {
            var intervalsByEmployee = projectGroup
                                        .GroupBy(a => a.EmployeeId)
                                        .ToDictionary(
                                            g => g.Key,
                                            g => MergeOverlappingIntervals(g.Select(a => (a.Begining, a.End)).ToList()));

            var employeeIds = intervalsByEmployee.Keys.ToList();

            for (var i = 0; i < employeeIds.Count; i++)
            {
                for (var j = i + 1; j < employeeIds.Count; j++)
                {
                    var emp1 = employeeIds[i];
                    var emp2 = employeeIds[j];

                    var projectOverlapDays = 0;

                    foreach (var interval1 in intervalsByEmployee[emp1])
                    {
                        foreach (var interval2 in intervalsByEmployee[emp2])
                        {
                            var overlapStart = interval1.Start > interval2.Start ? interval1.Start : interval2.Start;
                            var overlapEnd = interval1.End < interval2.End ? interval1.End : interval2.End;

                            if (overlapStart <= overlapEnd)
                            {
                                projectOverlapDays += overlapEnd.DayNumber - overlapStart.DayNumber + 1;
                            }
                        }
                    }

                    if (projectOverlapDays > 0)
                    {
                        commonProjects.Add(new CommonProject(
                            Math.Min(emp1, emp2),
                            Math.Max(emp1, emp2),
                            projectGroup.Key)
                        { DaysWorked = projectOverlapDays });
                    }
                }
            }
        }

        var winner = commonProjects
                    .GroupBy(p => (p.EmployeeId1, p.EmployeeId2))
                    .MaxBy(g => g.Sum(p => p.DaysWorked));

        return winner is null
            ? null
            : new EmployeePair(
                winner.Key.EmployeeId1,
                winner.Key.EmployeeId2,
                winner.OrderBy(p => p.ProjectId).ToList());
    }

    private static List<(DateOnly Start, DateOnly End)> MergeOverlappingIntervals(List<(DateOnly Start, DateOnly End)> intervals)
    {
        if (intervals.Count <= 1) return intervals;

        var sorted = intervals.OrderBy(i => i.Start).ToList();
        var merged = new List<(DateOnly Start, DateOnly End)>();
        var current = sorted[0];

        for (var i = 1; i < sorted.Count; i++)
        {
            var next = sorted[i];

            if (next.Start <= current.End)
            {
                current.End = next.End > current.End ? next.End : current.End;
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }

        merged.Add(current);
        return merged;
    }
}