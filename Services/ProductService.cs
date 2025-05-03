using CompanyProductAPI.Models;
using CompanyProductAPI.Repositories;

namespace CompanyProductAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;

        public ProductService(IProductRepository productRepository, ICompanyRepository companyRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        }

        public async Task<IEnumerable<Product>> GetProductsByCompanyIdAsync(int companyId)
        {
            // First check if company exists
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                throw new KeyNotFoundException($"Company with ID {companyId} not found");
            }

            var products = await _productRepository.GetAllByCompanyIdAsync(companyId);

            // Utilizando LINQ para transformar los resultados
            return products
                .OrderByDescending(p => p.CreatedAt)
                .ThenBy(p => p.Name);
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateProductAsync(Product product)
        {
            ValidateProduct(product);

            // Check if company exists
            var company = await _companyRepository.GetByIdAsync(product.CompanyId);
            if (company == null)
            {
                throw new KeyNotFoundException($"Company with ID {product.CompanyId} not found");
            }

            return await _productRepository.CreateAsync(product);
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            if (product.Id <= 0)
            {
                throw new ArgumentException("Invalid product ID", nameof(product));
            }

            ValidateProduct(product);

            // Check if product exists first
            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                return false;
            }

            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid product ID");
            }

            return await _productRepository.DeleteAsync(id);
        }

        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product name is required", nameof(product));
            }

            if (product.CompanyId <= 0)
            {
                throw new ArgumentException("Invalid company ID", nameof(product));
            }

            if (product.Price < 0)
            {
                throw new ArgumentException("Product price cannot be negative", nameof(product));
            }

            if (product.Stock < 0)
            {
                throw new ArgumentException("Product stock cannot be negative", nameof(product));
            }
        }
    }
}
