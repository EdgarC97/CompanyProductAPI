using CompanyProductAPI.Models;
using CompanyProductAPI.Repositories;

namespace CompanyProductAPI.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();

            // Aquí podríamos aplicar LINQ para filtrar, ordenar o transformar la lista de compañías
            return companies.OrderBy(c => c.Name);
        }

        public async Task<Company> GetCompanyByIdAsync(int id)
        {
            return await _companyRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateCompanyAsync(Company company)
        {
            if (string.IsNullOrWhiteSpace(company.Name))
            {
                throw new ArgumentException("Company name is required", nameof(company));
            }

            return await _companyRepository.CreateAsync(company);
        }

        public async Task<bool> UpdateCompanyAsync(Company company)
        {
            if (company.Id <= 0)
            {
                throw new ArgumentException("Invalid company ID", nameof(company));
            }

            if (string.IsNullOrWhiteSpace(company.Name))
            {
                throw new ArgumentException("Company name is required", nameof(company));
            }

            return await _companyRepository.UpdateAsync(company);
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid company ID");
            }

            return await _companyRepository.DeleteAsync(id);
        }
    }
}
