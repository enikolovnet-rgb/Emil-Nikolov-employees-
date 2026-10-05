using EmployeePairs.Domain.Entities;
using NUnit.Framework;

[TestFixture]
public class EmployeeOverlapServiceTests
{
    [Test]
    public void TestFile_ReturnsPairWithProjectBreakdown()
    {
        var assignments = new List<EmployeeProjectAssignment>
        {
            new(101, 1, new(2020, 1, 1), new(2020, 12, 31)),
            new(102, 1, new(2020, 6, 1), new(2021, 6, 1)),
            new(101, 2, new(2021, 1, 1), new(2021, 6, 30)),
            new(102, 2, new(2021, 3, 1), new(2026, 10, 5)),
            new(101, 3, new(2018, 1, 1), new(2018, 3, 31)),
            new(101, 3, new(2018, 9, 1), new(2018, 12, 31)),
            new(102, 3, new(2018, 2, 1), new(2018, 10, 31)),
            new(103, 4, new(2019, 1, 1), new(2019, 8, 31)),
            new(104, 4, new(2019, 3, 1), new(2019, 12, 31)),
        };

        var result = EmployeeOverlapService.CalculateExactOverlapDays(assignments);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.EmployeeId1, Is.EqualTo(101));
            Assert.That(result.EmployeeId2, Is.EqualTo(102));
            Assert.That(result.TotalDaysWorked, Is.EqualTo(456));
            Assert.That(result.CommonProjects.Select(p => (p.ProjectId, p.DaysWorked)),
                Is.EqualTo(new[] { (1, 214), (2, 122), (3, 120) }));
        });
    }

    [Test]
    public void NoOverlap_ReturnsNull()
    {
        var assignments = new List<EmployeeProjectAssignment>
        {
            new(1, 5, new(2022, 1, 1), new(2022, 6, 30)),
            new(2, 5, new(2022, 7, 1), new(2022, 12, 31)),
        };

        Assert.That(EmployeeOverlapService.CalculateExactOverlapDays(assignments), Is.Null);
    }

    [Test]
    public void SingleSharedDay_CountsAsOneDay()
    {
        var assignments = new List<EmployeeProjectAssignment>
        {
            new(7, 8, new(2015, 1, 1), new(2015, 1, 1)),
            new(8, 8, new(2015, 1, 1), new(2015, 12, 31)),
        };

        var result = EmployeeOverlapService.CalculateExactOverlapDays(assignments);

        Assert.That(result!.TotalDaysWorked, Is.EqualTo(1));
    }

    [Test]
    public void DuplicateRowsForSameEmployee_AreNotCountedTwice()
    {
        var assignments = new List<EmployeeProjectAssignment>
        {
            new(1, 9, new(2020, 1, 1), new(2020, 1, 10)),
            new(1, 9, new(2020, 1, 1), new(2020, 1, 10)), // duplicate
            new(2, 9, new(2020, 1, 1), new(2020, 1, 10)),
        };

        var result = EmployeeOverlapService.CalculateExactOverlapDays(assignments);

        Assert.That(result!.TotalDaysWorked, Is.EqualTo(10));
    }
}