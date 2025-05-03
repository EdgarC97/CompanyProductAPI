using CompanyProductAPI.Models;

namespace CompanyProductAPI.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsByCompanyIdAsync(int companyId);
        Task<Product> GetProductByIdAsync(int id);
        Task<int> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
    }
}
