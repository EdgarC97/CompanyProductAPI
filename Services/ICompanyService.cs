using CompanyProductAPI.Models;

namespace CompanyProductAPI.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company> GetCompanyByIdAsync(int id);
        Task<int> CreateCompanyAsync(Company company);
        Task<bool> UpdateCompanyAsync(Company company);
        Task<bool> DeleteCompanyAsync(int id);
    }
}
