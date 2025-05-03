using CompanyProductAPI.Models;

namespace CompanyProductAPI.Data
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllByCompanyIdAsync(int companyId);
        Task<Product> GetByIdAsync(int id);
        Task<int> CreateAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
    }
}
