using Scanitech_DataAccess.Entities;

namespace Scanitech_DataAccess.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<CustomerEntity>> GetAllAsync();
}