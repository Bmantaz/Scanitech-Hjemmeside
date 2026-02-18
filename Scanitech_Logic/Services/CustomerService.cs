using Scanitech_DataAccess.Interfaces;

namespace Scanitech_Logic.Services;

public sealed class CustomerService
{
    private readonly ICustomerRepository _repo;
    public CustomerService(ICustomerRepository repo) => _repo = repo;
}