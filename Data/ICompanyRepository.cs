using CompanyProductAPI.Models;

namespace CompanyProductAPI.Data
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> GetAllAsync();
        Task<Company> GetByIdAsync(int id);
        Task<Company> GetByIdWithProductsAsync(int id);
        Task<int> CreateAsync(Company company);
        Task<bool> UpdateAsync(Company company);
        Task<bool> DeleteAsync(int id);
    }
}
