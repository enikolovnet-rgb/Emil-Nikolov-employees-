using EmployeePairs.Domain.Entities;

namespace EmployeePairs.Application.Services
{
    public interface IEmployeePairService
    {
        Task<EmployeePair?> FindLongestWorkingPairAsync(Stream content, CancellationToken cancellationToken = default);
    }
}
