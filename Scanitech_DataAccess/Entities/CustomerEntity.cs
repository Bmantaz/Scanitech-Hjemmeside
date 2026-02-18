using Scanitech_DataAccess.Interfaces;
using Scanitech_DataAccess.Entities;
using Scanitech_Logic.Configuration;

namespace Scanitech_DataAccess.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IDatabaseInfo _dbInfo;
    public CustomerRepository(IDatabaseInfo dbInfo) => _dbInfo = dbInfo;

    public Task<CustomerEntity?> GetByIdAsync(int id) => throw new NotImplementedException();
    public Task<IReadOnlyList<CustomerEntity>> GetAllAsync() => throw new NotImplementedException();
}