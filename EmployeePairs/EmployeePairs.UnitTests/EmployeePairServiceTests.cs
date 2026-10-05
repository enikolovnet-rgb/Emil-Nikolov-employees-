using EmployeePairs.Application.Exceptions;
using EmployeePairs.Application.Services;
using Moq;
using System.Text;

[TestFixture]
public class EmployeePairServiceTests
{
    private Mock<TimeProvider> _timeProvider = null!;
    private IEmployeePairService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _timeProvider = new Mock<TimeProvider>();
        _timeProvider.Setup(t => t.GetUtcNow())
            .Returns(new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero));
        _timeProvider.Setup(t => t.LocalTimeZone).Returns(TimeZoneInfo.Utc);

        _service = new EmployeePairService(_timeProvider.Object);
    }

    private static Stream ToStream(string csv) => new MemoryStream(Encoding.UTF8.GetBytes(csv));

    [Test]
    public async Task NullDateTo_IsResolvedToToday()
    {
        const string csv = """
            EmpID, ProjectID, DateFrom, DateTo
            1, 10, 2026-01-01, NULL
            2, 10, 2026-03-01, NULL
            """;

        var result = await _service.FindLongestWorkingPairAsync(ToStream(csv), CancellationToken.None);

        // 2026-03-01 .. 2026-03-10 inclusive = 10 days
        Assert.That(result!.TotalDaysWorked, Is.EqualTo(10));
        _timeProvider.Verify(t => t.GetUtcNow(), Times.AtLeastOnce);
    }

    [Test]
    public async Task MixedDateFormats_AreParsed()
    {
        const string csv = """
            EmpID, ProjectID, DateFrom, DateTo
            105, 6, 01.05.2023, 31.12.2023
            106, 6, 2023/03/15, 2023/10/15
            """;

        var result = await _service.FindLongestWorkingPairAsync(ToStream(csv), CancellationToken.None);

        // 2023-05-01 .. 2023-10-15 inclusive = 168 days
        Assert.That(result!.TotalDaysWorked, Is.EqualTo(168));
    }

    [Test]
    public void HeaderOnly_ThrowsInvalidInputFileException()
    {
        const string csv = "EmpID, ProjectID, DateFrom, DateTo";

        Assert.ThrowsAsync<InvalidInputFileException>(() =>
            _service.FindLongestWorkingPairAsync(ToStream(csv), CancellationToken.None));
    }
}